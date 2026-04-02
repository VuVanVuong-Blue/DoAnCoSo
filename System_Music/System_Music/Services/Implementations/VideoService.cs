using System_Music.Models.SqlModels;
using System_Music.Models.DTOs;
using System.Threading.Tasks;
using System.Net.Http;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using System_Music.Services.Interfaces;
using AutoMapper;
using System;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using System.Linq;

namespace System_Music.Services.Implementations
{
    public class VideoService : IVideoService
    {
        private readonly IVideoRepository _videoRepository;
        private readonly SmartMusicDbContext _context;
        private readonly HttpClient _httpClient;
        private readonly string _zingMp3ApiBaseUrl;
        private readonly ILogger<VideoService> _logger;
        private readonly IServiceProvider _serviceProvider;
        private readonly IMapper _mapper;

        public VideoService(
            IVideoRepository videoRepository, 
            SmartMusicDbContext context, 
            IHttpClientFactory httpClientFactory, 
            IConfiguration configuration, 
            ILogger<VideoService> logger, 
            IServiceProvider serviceProvider,
            IMapper mapper)
        {
            _videoRepository = videoRepository;
            _context = context;
            _httpClient = httpClientFactory.CreateClient();
            _zingMp3ApiBaseUrl = configuration["ZingMp3Api:BaseUrl"];
            _logger = logger;
            _serviceProvider = serviceProvider;
            _mapper = mapper;
        }

        public async Task<VideoDto> GetVideoByIdAsync(string encodeId)
        {
            var existingVideo = await _context.Videos
                .Include(v => v.VideoArtists)
                .ThenInclude(va => va.Artist)
                .FirstOrDefaultAsync(v => v.EncodeId == encodeId);

            bool shouldUpdate = true;

            if (existingVideo != null)
            {
                _logger.LogInformation("Video {EncodeId} found in database", encodeId);
                if (!string.IsNullOrEmpty(existingVideo.Hls))
                {
                    try 
                    {
                        var uri = new Uri(existingVideo.Hls);
                        var query = System.Web.HttpUtility.ParseQueryString(uri.Query);
                        if (query["authen"] != null)
                        {
                            var authenParts = query["authen"].Split('~');
                            var expPart = authenParts.FirstOrDefault(p => p.StartsWith("exp="));
                            if (expPart != null && long.TryParse(expPart.Replace("exp=", ""), out var exp))
                            {
                                var expDate = DateTimeOffset.FromUnixTimeSeconds(exp).UtcDateTime;
                                if (expDate > DateTime.UtcNow)
                                {
                                    _logger.LogInformation("HLS URL for {EncodeId} is still valid, using cached data", encodeId);
                                    shouldUpdate = false;
                                    return _mapper.Map<VideoDto>(existingVideo);
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Failed to parse HLS URL authentication for {EncodeId}", encodeId);
                    }
                }
                _logger.LogInformation("HLS URL for {EncodeId} is expired or invalid, attempting to fetch new data", encodeId);
            }

            if (shouldUpdate)
            {
                var url = $"{_zingMp3ApiBaseUrl}/api/video/{encodeId}";
                try
                {
                    var response = await _httpClient.GetAsync(url);
                    if (response.IsSuccessStatusCode)
                    {
                        var jsonString = await response.Content.ReadAsStringAsync();
                        var apiResponse = JsonSerializer.Deserialize<ZingMp3ApiResponse>(jsonString);
                        if (apiResponse != null && apiResponse.err == 0 && apiResponse.data != null)
                        {
                            var videoData = apiResponse.data;
                            string hlsUrl = null;
                            if (videoData.streaming?.hls != null)
                            {
                                hlsUrl = videoData.streaming.hls.OrderByDescending(kvp => kvp.Key).Select(kvp => kvp.Value).FirstOrDefault();
                            }

                            var video = existingVideo ?? new Video { EncodeId = videoData.encodeId };
                            video.Title = videoData.title;
                            video.Thumbnail = videoData.thumbnail;
                            video.ThumbnailM = videoData.thumbnailM;
                            video.Duration = videoData.duration;
                            video.ArtistsNames = videoData.artistsNames;
                            video.Link = videoData.link;
                            video.Mp4_480 = videoData.streaming?.mp4_480 ?? string.Empty;
                            video.Mp4_720 = videoData.streaming?.mp4_720 ?? string.Empty;
                            video.Mp4_1080 = videoData.streaming?.mp4_1080 ?? string.Empty;
                            video.Hls = hlsUrl ?? string.Empty;
                            video.UpdatedDate = DateTime.UtcNow;

                            if (existingVideo == null)
                                _context.Videos.Add(video);
                            else
                                _context.Videos.Update(video);

                            await _context.SaveChangesAsync();
                            
                            // Sync Artists
                            if (videoData.artists != null)
                            {
                                var artistService = _serviceProvider.GetService<IArtistService>();
                                if (artistService != null)
                                {
                                    foreach (var a in videoData.artists)
                                    {
                                        await artistService.SyncArtistsFromZingMp3Async(null, a.id);
                                    }
                                }
                            }

                            return _mapper.Map<VideoDto>(video);
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error fetching video {EncodeId} from Zing API", encodeId);
                }
            }

            return existingVideo != null ? _mapper.Map<VideoDto>(existingVideo) : null;
        }

        public async Task<IEnumerable<VideoDto>> GetAllVideosAsync()
        {
            var videos = await _context.Videos
                .Include(v => v.VideoArtists)
                .ThenInclude(va => va.Artist)
                .ToListAsync();
            return _mapper.Map<IEnumerable<VideoDto>>(videos);
        }

        public async Task AddVideoAsync(VideoDto videoDto)
        {
            var video = _mapper.Map<Video>(videoDto);
            _context.Videos.Add(video);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateVideoAsync(VideoDto videoDto)
        {
            var video = _mapper.Map<Video>(videoDto);
            _context.Videos.Update(video);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteVideoAsync(string encodeId)
        {
            var video = await _context.Videos.FindAsync(encodeId);
            if (video != null)
            {
                _context.Videos.Remove(video);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<IEnumerable<VideoDto>> GetRelatedVideosAsync(string videoId)
        {
            var artistIds = await _context.VideoArtists
                .Where(va => va.VideoEncodeId == videoId)
                .Select(va => va.ArtistId)
                .ToListAsync();

            List<Video> related;
            if (artistIds.Any())
            {
                related = await _context.Videos
                    .Include(v => v.VideoArtists)
                    .ThenInclude(va => va.Artist)
                    .Where(v => v.EncodeId != videoId && v.VideoArtists.Any(va => artistIds.Contains(va.ArtistId)))
                    .Take(10)
                    .ToListAsync();
            }
            else
            {
                related = await _context.Videos
                    .Include(v => v.VideoArtists)
                    .ThenInclude(va => va.Artist)
                    .Where(v => v.EncodeId != videoId)
                    .OrderBy(v => Guid.NewGuid())
                    .Take(10)
                    .ToListAsync();
            }

            return _mapper.Map<IEnumerable<VideoDto>>(related);
        }

        public class ZingMp3ApiResponse
        {
            public int err { get; set; }
            public string msg { get; set; }
            public ZingMp3VideoData data { get; set; }
        }

        public class ZingMp3VideoData
        {
            public string encodeId { get; set; }
            public string title { get; set; }
            public string thumbnail { get; set; }
            public string thumbnailM { get; set; }
            public int duration { get; set; }
            public List<ZingMp3Artist> artists { get; set; }
            public string artistsNames { get; set; }
            public ZingMp3Streaming streaming { get; set; }
            public string link { get; set; }
        }

        public class ZingMp3Artist
        {
            public string id { get; set; }
            public string name { get; set; }
        }

        public class ZingMp3Streaming
        {
            public string mp4_480 { get; set; }
            public string mp4_720 { get; set; }
            public string mp4_1080 { get; set; }
            public Dictionary<string, string> hls { get; set; }
        }
    }
}
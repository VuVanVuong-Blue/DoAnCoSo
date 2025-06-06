using System_Music.Models.SqlModels;
using System_Music.Interfaces;
using System.Threading.Tasks;
using System.Net.Http;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using System_Music.Services.Interfaces;
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

        public VideoService(IVideoRepository videoRepository, SmartMusicDbContext context, IHttpClientFactory httpClientFactory, IConfiguration configuration, ILogger<VideoService> logger, IServiceProvider serviceProvider)
        {
            _videoRepository = videoRepository;
            _context = context;
            _httpClient = httpClientFactory.CreateClient();
            _zingMp3ApiBaseUrl = configuration["ZingMp3Api:BaseUrl"];
            _logger = logger;
            _serviceProvider = serviceProvider;
        }

        public async Task<Video> GetVideoByIdAsync(string encodeId)
        {
            var existingVideo = await _videoRepository.GetVideoByIdAsync(encodeId);
            bool shouldUpdate = true;

            if (existingVideo != null)
            {
                _logger.LogInformation("Video {EncodeId} found in database", encodeId);
                if (!string.IsNullOrEmpty(existingVideo.Hls))
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
                                return existingVideo;
                            }
                        }
                    }
                }
                _logger.LogInformation("HLS URL for {EncodeId} is expired or invalid, attempting to fetch new data", encodeId);
            }

            if (shouldUpdate)
            {
                var url = $"{_zingMp3ApiBaseUrl}/api/video/{encodeId}";
                _logger.LogInformation("Calling Zing MP3 API for video {EncodeId} at {Url}", encodeId, url);

                try
                {
                    var response = await _httpClient.GetAsync(url);
                    if (!response.IsSuccessStatusCode)
                    {
                        _logger.LogError("Failed to fetch video {EncodeId} from Zing MP3 API. Status: {StatusCode}", encodeId, response.StatusCode);
                        if (existingVideo != null)
                        {
                            _logger.LogWarning("Returning cached video {EncodeId} due to API failure", encodeId);
                            return existingVideo; // Trả về video cũ nếu API thất bại
                        }
                        throw new HttpRequestException("Không thể lấy thông tin MV từ API");
                    }

                    var jsonString = await response.Content.ReadAsStringAsync();
                    _logger.LogInformation("API response for {EncodeId}: {Response}", encodeId, jsonString);

                    var apiResponse = JsonSerializer.Deserialize<ZingMp3ApiResponse>(jsonString);
                    if (apiResponse.err != 0 || apiResponse.data == null)
                    {
                        _logger.LogWarning("No video data found for encodeId {EncodeId}. Error: {Error}, Message: {Message}", encodeId, apiResponse.err, apiResponse.msg);
                        if (existingVideo != null)
                        {
                            _logger.LogWarning("Returning cached video {EncodeId} due to invalid API response", encodeId);
                            return existingVideo; // Trả về video cũ nếu API trả về lỗi
                        }
                        throw new Exception("Không tìm thấy video MV");
                    }

                    var videoData = apiResponse.data;

                    string hlsUrl = null;
                    var hlsDict = videoData.streaming?.hls;
                    if (hlsDict != null)
                    {
                        hlsUrl = hlsDict.Where(kvp => !string.IsNullOrEmpty(kvp.Value))
                                       .OrderByDescending(kvp => kvp.Key switch
                                       {
                                           "1080p" => 3,
                                           "720p" => 2,
                                           "360p" => 1,
                                           _ => 0
                                       })
                                       .Select(kvp => kvp.Value)
                                       .FirstOrDefault();
                        _logger.LogInformation("Selected HLS URL for {EncodeId}: {HlsUrl}", encodeId, hlsUrl);
                    }

                    var video = new Video
                    {
                        EncodeId = videoData.encodeId,
                        Title = videoData.title,
                        Thumbnail = videoData.thumbnail,
                        ThumbnailM = videoData.thumbnailM,
                        Duration = videoData.duration,
                        ArtistsNames = videoData.artistsNames,
                        Link = videoData.link,
                        Mp4_480 = videoData.streaming?.mp4_480 ?? string.Empty,
                        Mp4_720 = videoData.streaming?.mp4_720 ?? string.Empty,
                        Mp4_1080 = videoData.streaming?.mp4_1080 ?? string.Empty,
                        Hls = hlsUrl ?? string.Empty
                    };

                    try
                    {
                        await _videoRepository.AddOrUpdateVideoAsync(video);
                        _logger.LogInformation("Video {EncodeId} updated in database with new HLS URL", video.EncodeId);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Failed to update video {EncodeId} in database", video.EncodeId);
                        if (existingVideo != null)
                        {
                            _logger.LogWarning("Returning cached video {EncodeId} due to database update failure", encodeId);
                            return existingVideo; // Trả về video cũ nếu cập nhật thất bại
                        }
                        throw;
                    }

                    if (videoData.artists != null && videoData.artists.Any())
                    {
                        var artistService = _serviceProvider.GetService<IArtistService>();
                        if (artistService == null) throw new InvalidOperationException("ArtistService not available");

                        var videoArtists = new List<VideoArtist>();
                        foreach (var artist in videoData.artists)
                        {
                            await artistService.SyncArtistsFromZingMp3Async(null, artist.id);
                            var syncedArtist = (await artistService.GetAllArtistsAsync())
                                .FirstOrDefault(a => a.ZingMp3ArtistId == artist.id);

                            if (syncedArtist != null)
                            {
                                videoArtists.Add(new VideoArtist
                                {
                                    VideoEncodeId = video.EncodeId,
                                    ArtistId = syncedArtist.ArtistId
                                });
                            }
                            else
                            {
                                _logger.LogWarning("Failed to sync artist {ArtistId} for video {EncodeId}, skipping", artist.id, video.EncodeId);
                            }
                        }

                        if (videoArtists.Any())
                        {
                            try
                            {
                                await _videoRepository.AddVideoArtistsAsync(videoArtists);
                                _logger.LogInformation("Added {Count} VideoArtist records for {EncodeId}", videoArtists.Count, video.EncodeId);
                            }
                            catch (Exception ex)
                            {
                                _logger.LogError(ex, "Failed to add VideoArtists for {EncodeId}", video.EncodeId);
                                throw;
                            }
                        }
                        else
                        {
                            _logger.LogWarning("No valid artists synced for video {EncodeId}", video.EncodeId);
                        }
                    }
                    return video;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error fetching video {EncodeId} from API", encodeId);
                    if (existingVideo != null)
                    {
                        _logger.LogWarning("Returning cached video {EncodeId} due to API error", encodeId);
                        return existingVideo; // Trả về video cũ nếu có lỗi
                    }
                    throw;
                }
            }

            return existingVideo;
        }

        public async Task<IEnumerable<Video>> GetAllVideosAsync()
        {
            var videos = await _context.Videos
                .Include(v => v.VideoArtists)
                .ThenInclude(va => va.Artist)
                .ToListAsync();
            foreach (var video in videos)
            {
                Console.WriteLine($"Video {video.EncodeId} has {video.VideoArtists?.Count ?? 0} artists.");
                foreach (var va in video.VideoArtists ?? new List<VideoArtist>())
                {
                    Console.WriteLine($"Artist: {va.Artist?.Name}, Image: {va.Artist?.Image}");
                }
            }
            return videos ?? Enumerable.Empty<Video>();
        }

        public async Task AddVideoAsync(Video video)
        {
            _context.Videos.Add(video);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateVideoAsync(Video video)
        {
            _context.Videos.Update(video);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteVideoAsync(string encodeId)
        {
            var video = await _videoRepository.GetVideoByIdAsync(encodeId);
            if (video == null)
            {
                _logger.LogWarning("Video {EncodeId} not found for deletion", encodeId);
                throw new KeyNotFoundException($"Video with ID {encodeId} not found.");
            }

            var videoArtists = await _context.VideoArtists
                .Where(va => va.VideoEncodeId == encodeId)
                .ToListAsync();
            if (videoArtists.Any())
            {
                _context.VideoArtists.RemoveRange(videoArtists);
                await _context.SaveChangesAsync();
                _logger.LogInformation("Removed {Count} VideoArtist records for {EncodeId}", videoArtists.Count, encodeId);
            }

            await _videoRepository.DeleteVideoAsync(encodeId);
            _logger.LogInformation("Video {EncodeId} deleted successfully", encodeId);
        }

        public async Task<IEnumerable<Video>> GetRelatedVideosAsync(string videoId)
        {
            var video = await _videoRepository.GetVideoByIdAsync(videoId);
            if (video == null)
            {
                _logger.LogWarning("No video found with encodeId {EncodeId}", videoId);
                return Enumerable.Empty<Video>();
            }

            // Lấy danh sách artistId của video hiện tại
            var artistIds = await _context.VideoArtists
                .Where(va => va.VideoEncodeId == videoId)
                .Select(va => va.ArtistId)
                .ToListAsync();

            if (!artistIds.Any())
            {
                _logger.LogInformation("No artists found for video {EncodeId}, fetching random videos", videoId);
                return await _context.Videos
                    .Include(v => v.VideoArtists)
                    .ThenInclude(va => va.Artist)
                    .Where(v => v.EncodeId != videoId)
                    .OrderBy(r => Guid.NewGuid())
                    .Take(10)
                    .ToListAsync();
            }

            // Lấy các video có cùng artistId
            var relatedVideos = await _context.Videos
                .Include(v => v.VideoArtists)
                .ThenInclude(va => va.Artist)
                .Where(v => v.VideoArtists.Any(va => artistIds.Contains(va.ArtistId))
                         && v.EncodeId != videoId)
                .Take(10)
                .ToListAsync();

            if (!relatedVideos.Any())
            {
                _logger.LogInformation("No related videos found by artist for {EncodeId}, fetching random videos", videoId);
                relatedVideos = await _context.Videos
                    .Include(v => v.VideoArtists)
                    .ThenInclude(va => va.Artist)
                    .Where(v => v.EncodeId != videoId)
                    .OrderBy(r => Guid.NewGuid())
                    .Take(10)
                    .ToListAsync();
            }

            return relatedVideos ?? Enumerable.Empty<Video>();
        }

        public class ZingMp3ApiResponse
        {
            public int err { get; set; }
            public string msg { get; set; }
            public ZingMp3VideoData data { get; set; }
            public long timestamp { get; set; }
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
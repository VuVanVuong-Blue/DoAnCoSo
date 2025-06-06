using Microsoft.EntityFrameworkCore;
using System_Music.Models.SqlModels;
using System_Music.Repositories.Interfaces;
using System.Text.Json;
using System.Text;
using System.Net.Http;
using Microsoft.Extensions.Configuration;
using System.Globalization;

namespace System_Music.Repositories.Implementations
{
    public class AlbumRepository : Repository<Album>, IAlbumRepository
    {
        private readonly SmartMusicDbContext _context;
        private readonly HttpClient _httpClient;
        private readonly string _zingMp3ApiUrl;
        private readonly ITrackRepository _trackRepository;

        public AlbumRepository(SmartMusicDbContext context, HttpClient httpClient, IConfiguration configuration, ITrackRepository trackRepository) : base(context)
        {
            _context = context;
            _httpClient = httpClient;
            _zingMp3ApiUrl = configuration["ZingMp3ApiUrl"] ?? "http://localhost:5000";
            _trackRepository = trackRepository;
        }

        public Task<List<Album>> GetAlbumsByArtistAsync(int artistId)
        {
            return _context.Albums
                .Where(a => a.AlbumArtists.Any(aa => aa.ArtistId == artistId))
                .Include(a => a.Tracks)
                .Include(a => a.AlbumArtists)
                    .ThenInclude(aa => aa.Artist)
                .ToListAsync();
        }

        public override async Task<List<Album>> GetAllAsync()
        {
            return await _context.Albums
                .Include(a => a.Tracks)
                .Include(a => a.AlbumArtists)
                    .ThenInclude(aa => aa.Artist)
                .ToListAsync();
        }

        public async Task<Album> GetByIdAsync(int id)
        {
            return await _context.Albums
                .Include(a => a.Tracks)
                .Include(a => a.AlbumArtists)
                    .ThenInclude(aa => aa.Artist)
                .FirstOrDefaultAsync(a => a.AlbumId == id);
        }

        public async Task<Album> GetByIdWithDetailsAsync(int id)
        {
            return await _context.Albums
                .Include(a => a.Tracks)
                    .ThenInclude(t => t.TrackArtists)
                        .ThenInclude(ta => ta.Artist)
                .Include(a => a.Tracks)
                    .ThenInclude(t => t.TrackGenres)
                        .ThenInclude(tg => tg.Genre)
                .Include(a => a.AlbumArtists)
                    .ThenInclude(aa => aa.Artist)
                .FirstOrDefaultAsync(a => a.AlbumId == id);
        }

        public async Task<List<Album>> SyncAlbumFromZingMp3Async(string albumEncodeId)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(albumEncodeId))
                {
                    Console.WriteLine("[DEBUG] Album EncodeId is empty or null.");
                    return new List<Album>();
                }

                var albumUrl = $"{_zingMp3ApiUrl}/api/album/{albumEncodeId}";
                Console.WriteLine($"[DEBUG] Calling Node.js API - Album: {albumUrl}");

                var response = await _httpClient.GetAsync(albumUrl);
                if (!response.IsSuccessStatusCode)
                {
                    Console.WriteLine($"[DEBUG] API call to /api/album failed: Status={response.StatusCode}");
                    return new List<Album>();
                }

                var json = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"[DEBUG] Raw Response from /api/album: {json}");
                var doc = JsonDocument.Parse(json);
                if (doc.RootElement.GetProperty("err").GetInt32() != 0)
                {
                    Console.WriteLine($"[DEBUG] API error for /api/album: {doc.RootElement.GetProperty("msg").GetString()}");
                    return new List<Album>();
                }

                var albums = new List<Album>();
                var albumData = doc.RootElement.GetProperty("data");

                // Kiểm tra xem album đã tồn tại chưa
                var existingAlbum = await _context.Albums
                    .FirstOrDefaultAsync(a => a.ZingMp3AlbumId == albumEncodeId);

                if (existingAlbum != null)
                {
                    Console.WriteLine($"[DEBUG] Album with encodeId {albumEncodeId} already exists in database.");
                    albums.Add(existingAlbum);
                    return albums;
                }

                // Xử lý releaseDate
                DateTime releaseDate = DateTime.UtcNow;
                if (albumData.TryGetProperty("releaseDate", out var releaseDateElement))
                {
                    if (releaseDateElement.ValueKind == JsonValueKind.Number)
                    {
                        long timestamp = releaseDateElement.GetInt64();
                        DateTime epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
                        releaseDate = epoch.AddSeconds(timestamp).ToUniversalTime();
                        Console.WriteLine($"[DEBUG] Converted timestamp {timestamp} to releaseDate: {releaseDate}");
                    }
                    else if (releaseDateElement.ValueKind == JsonValueKind.String)
                    {
                        var releaseDateStr = releaseDateElement.GetString();
                        if (!string.IsNullOrEmpty(releaseDateStr) && !DateTime.TryParse(releaseDateStr, out releaseDate))
                        {
                            Console.WriteLine($"[DEBUG] Invalid releaseDate format: {releaseDateStr}. Using default: {releaseDate}");
                        }
                    }
                }

                // Xử lý nghệ sĩ
                var artistIds = new List<int>();
                if (albumData.TryGetProperty("artists", out var artistsElement) && artistsElement.ValueKind == JsonValueKind.Array)
                {
                    foreach (var artist in artistsElement.EnumerateArray())
                    {
                        if (artist.TryGetProperty("id", out var artistIdElement) && artist.TryGetProperty("name", out var artistNameElement))
                        {
                            var artistZingId = artistIdElement.GetString();
                            var artistName = artistNameElement.GetString();
                            var artistImage = artist.TryGetProperty("thumbnail", out var thumbnail) ? thumbnail.GetString() : null;

                            var existingArtist = await _context.Artists
                                .FirstOrDefaultAsync(a => a.ZingMp3ArtistId == artistZingId);

                            if (existingArtist == null)
                            {
                                existingArtist = new Artist
                                {
                                    Name = artistName,
                                    NormalizedName = RemoveDiacritics(artistName),
                                    Image = artistImage,
                                    ZingMp3ArtistId = artistZingId,
                                    CreatedDate = DateTime.UtcNow,
                                    IsActive = true
                                };
                                await _context.Artists.AddAsync(existingArtist);
                                await _context.SaveChangesAsync();
                            }
                            artistIds.Add(existingArtist.ArtistId);
                        }
                    }
                }

                // Tạo album mới
                var album = new Album
                {
                    Name = albumData.GetProperty("title").GetString(),
                    NormalizedName = RemoveDiacritics(albumData.GetProperty("title").GetString()),
                    Image = albumData.GetProperty("thumbnailM").GetString(),
                    ReleaseDate = releaseDate,
                    ZingMp3AlbumId = albumEncodeId,
                    CreatedDate = DateTime.UtcNow,
                    UpdatedDate = DateTime.UtcNow
                };

                await _context.Albums.AddAsync(album);
                await _context.SaveChangesAsync();

                // Liên kết nghệ sĩ với album
                foreach (var artistId in artistIds)
                {
                    var albumArtist = new AlbumArtist
                    {
                        AlbumId = album.AlbumId,
                        ArtistId = artistId
                    };
                    _context.AlbumArtists.Add(albumArtist);
                }
                await _context.SaveChangesAsync();

                // Đồng bộ các bài hát trong album
                if (albumData.TryGetProperty("songs", out var songsElement) && songsElement.ValueKind == JsonValueKind.Array)
                {
                    foreach (var song in songsElement.EnumerateArray())
                    {
                        if (song.TryGetProperty("encodeId", out var songEncodeIdElement))
                        {
                            var songEncodeId = songEncodeIdElement.GetString();
                            Console.WriteLine($"[DEBUG] Syncing track with encodeId: {songEncodeId}");
                            var tracks = await _trackRepository.SyncTracksFromZingMp3Async(songEncodeId);
                            foreach (var track in tracks)
                            {
                                track.AlbumId = album.AlbumId;
                                _context.Tracks.Update(track);
                            }
                        }
                    }
                    await _context.SaveChangesAsync();
                }

                albums.Add(album);
                Console.WriteLine($"[DEBUG] Successfully synced album: {album.Name} (ID: {album.ZingMp3AlbumId})");
                return albums;
            }
            catch (JsonException ex)
            {
                Console.WriteLine($"[DEBUG] JSON parsing error: {ex.Message}");
                throw new Exception($"Lỗi phân tích JSON: {ex.Message}", ex);
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"[DEBUG] HTTP request error: {ex.Message}");
                throw new Exception($"Lỗi kết nối tới API Zing MP3: {ex.Message}", ex);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[DEBUG] General error syncing album: {ex.Message} - StackTrace: {ex.StackTrace}");
                throw new Exception($"Lỗi đồng bộ album: {ex.Message}", ex);
            }
        }

        private string RemoveDiacritics(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return text;

            var normalizedString = text.Normalize(NormalizationForm.FormD);
            var stringBuilder = new StringBuilder();

            foreach (var c in normalizedString)
            {
                var unicodeCategory = CharUnicodeInfo.GetUnicodeCategory(c);
                if (unicodeCategory != UnicodeCategory.NonSpacingMark)
                {
                    stringBuilder.Append(c);
                }
            }

            return stringBuilder.ToString().Normalize(NormalizationForm.FormC).ToLower().Trim();
        }
    }
}
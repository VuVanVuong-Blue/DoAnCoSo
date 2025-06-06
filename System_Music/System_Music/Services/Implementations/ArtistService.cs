using System_Music.Models.SqlModels;
using System_Music.Repositories.Interfaces;
using System_Music.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Text;
using System.Globalization;
using System.Net.Http;
using Newtonsoft.Json;
using Microsoft.Extensions.Configuration;

namespace System_Music.Services.Implementations
{
    public class ArtistService : IArtistService
    {
        private readonly IArtistRepository _artistRepository;
        private readonly SmartMusicDbContext _context;
        private readonly HttpClient _httpClient;
        private readonly string _zingMp3ApiUrl;

        public ArtistService(IArtistRepository artistRepository, SmartMusicDbContext context, HttpClient httpClient, IConfiguration configuration)
        {
            _artistRepository = artistRepository;
            _context = context;
            _httpClient = httpClient;
            _zingMp3ApiUrl = configuration.GetSection("ZingMp3Api:BaseUrl").Value ?? "http://localhost:5000";
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

        public async Task<List<Artist>> GetAllArtistsAsync()
        {
            return await _artistRepository.GetAllAsync();
        }

        public async Task<Artist> GetArtistByIdAsync(int id)
        {
            return await _artistRepository.GetByIdAsync(id);
        }

        public async Task AddArtistAsync(Artist artist)
        {
            if (string.IsNullOrWhiteSpace(artist.Name))
            {
                throw new ArgumentException("Tên nghệ sĩ không được để trống.");
            }

            artist.NormalizedName = RemoveDiacritics(artist.Name);
            artist.CreatedDate = DateTime.UtcNow;
            artist.UpdatedDate = DateTime.UtcNow;

            await _artistRepository.AddAsync(artist);
        }

        public async Task UpdateArtistAsync(Artist artist)
        {
            var existingArtist = await _artistRepository.GetByIdAsync(artist.ArtistId);
            if (existingArtist == null)
            {
                throw new KeyNotFoundException($"Artist with ID {artist.ArtistId} not found.");
            }

            if (string.IsNullOrWhiteSpace(artist.Name))
            {
                throw new ArgumentException("Tên nghệ sĩ không được để trống.");
            }

            existingArtist.Name = artist.Name;
            existingArtist.NormalizedName = RemoveDiacritics(artist.Name);
            existingArtist.Country = artist.Country;
            existingArtist.Image = artist.Image;
            existingArtist.Bio = artist.Bio;
            existingArtist.BirthDate = artist.BirthDate;
            existingArtist.IsActive = artist.IsActive;
            existingArtist.UpdatedDate = DateTime.UtcNow;

            await _artistRepository.UpdateAsync(existingArtist);
        }

            public async Task DeleteArtistAsync(int id)
            {
                var artist = await _artistRepository.GetByIdAsync(id);
                if (artist == null)
                {
                    throw new KeyNotFoundException($"Artist with ID {id} not found.");
                }

                // Xóa các bản ghi trong TrackArtists liên quan đến Artist
                var trackArtists = await _context.TrackArtists
                    .Where(ta => ta.ArtistId == id)
                    .ToListAsync();
                if (trackArtists.Any())
                {
                    _context.TrackArtists.RemoveRange(trackArtists);
                    await _context.SaveChangesAsync();
                    Console.WriteLine($"[INFO] Removed {trackArtists.Count} TrackArtists records for Artist ID {id}");
                }

                // Xóa các bản ghi trong AlbumArtists liên quan đến Artist
                var albumArtists = await _context.AlbumArtists
                    .Where(aa => aa.ArtistId == id)
                    .ToListAsync();
                if (albumArtists.Any())
                {
                    _context.AlbumArtists.RemoveRange(albumArtists);
                    await _context.SaveChangesAsync();
                    Console.WriteLine($"[INFO] Removed {albumArtists.Count} AlbumArtists records for Artist ID {id}");
                }

                // Sau khi xóa các quan hệ, xóa Artist
                await _artistRepository.DeleteAsync(id);
                Console.WriteLine($"[INFO] Artist ID {id} deleted successfully");
            }       

        public async Task<List<Artist>> GetArtistsByCountryAsync(string country)
        {
            return await _artistRepository.GetArtistsByCountryAsync(country);
        }

        public async Task<List<Artist>> SearchArtistsAsync(string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
            {
                return new List<Artist>();
            }

            searchTerm = RemoveDiacritics(searchTerm) ?? searchTerm.ToLower().Trim();

            var artistsQuery = _context.Artists
                .AsNoTracking()
                .AsSplitQuery()
                .Include(a => a.TrackArtists)
                    .ThenInclude(ta => ta.Track)
                .Include(a => a.AlbumArtists)
                    .ThenInclude(aa => aa.Album)
                .Where(a =>
                    (a.NormalizedName != null && a.NormalizedName.Contains(searchTerm)) ||
                    (a.Country != null && EF.Functions.Like(a.Country.ToLower(), $"%{searchTerm}%")) ||
                    (a.Bio != null && EF.Functions.Like(a.Bio.ToLower(), $"%{searchTerm}%"))
                );

            var artists = await artistsQuery.ToListAsync();

            var scoredArtists = artists
                .Select(a => new
                {
                    Artist = a,
                    Score = CalculateRelevanceScore(a, searchTerm)
                })
                .OrderByDescending(x => x.Score)
                .Take(5)
                .Select(x => x.Artist)
                .ToList();

            return scoredArtists ?? new List<Artist>();
        }

        private double CalculateRelevanceScore(Artist artist, string searchTerm)
        {
            double score = 0;

            if (artist == null)
            {
                return score;
            }

            if (!string.IsNullOrEmpty(artist.NormalizedName))
            {
                if (artist.NormalizedName.Contains(searchTerm))
                {
                    score += 3.0;
                    if (artist.NormalizedName.StartsWith(searchTerm))
                    {
                        score += 1.0;
                    }
                }
            }

            if (!string.IsNullOrEmpty(artist.Country))
            {
                var normalizedCountry = artist.Country.ToLower();
                if (normalizedCountry.Contains(searchTerm))
                {
                    score += 1.0;
                }
            }

            if (!string.IsNullOrEmpty(artist.Bio))
            {
                var normalizedBio = artist.Bio.ToLower();
                if (normalizedBio.Contains(searchTerm))
                {
                    score += 1.0;
                }
            }

            if (artist.IsActive)
            {
                score += 0.5;
            }

            return score;
        }

        public async Task<List<Track>> GetTracksByArtistIdAsync(int artistId)
        {
            return await _context.Tracks
                .AsNoTracking()
                .Include(t => t.TrackArtists)
                    .ThenInclude(ta => ta.Artist)
                .Where(t => t.TrackArtists.Any(ta => ta.ArtistId == artistId))
                .OrderByDescending(t => t.PlayCount)
                .Take(10)
                .ToListAsync();
        }

        public async Task<List<Artist>> SyncArtistsFromZingMp3Async(string artistName = null, string artistId = null)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(artistName) && string.IsNullOrWhiteSpace(artistId))
                {
                    throw new ArgumentException("Phải cung cấp ít nhất một trong hai: tên nghệ sĩ hoặc ID nghệ sĩ.");
                }

                string url;
                if (!string.IsNullOrWhiteSpace(artistId))
                {
                    url = $"{_zingMp3ApiUrl}/api/artist-by-id/{Uri.EscapeDataString(artistId)}";
                }
                else
                {
                    url = $"{_zingMp3ApiUrl}/api/artist/{Uri.EscapeDataString(artistName)}";
                }

                var response = await _httpClient.GetAsync(url);
                var responseContent = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"[DEBUG] Response from {url}: Status={response.StatusCode}, Content={responseContent}");

                if (!response.IsSuccessStatusCode)
                {
                    Console.WriteLine($"[ERROR] Failed to fetch artist from {url}: Status={response.StatusCode}");
                    return await _artistRepository.GetAllAsync(); // Trả về danh sách hiện có
                }

                var jsonString = await response.Content.ReadAsStringAsync();
                var apiResponse = JsonConvert.DeserializeObject<ZingMp3ApiResponse>(jsonString);

                if (apiResponse.Err != 0 || apiResponse.Data == null || apiResponse.Data.data == null)
                {
                    Console.WriteLine($"[ERROR] Error from Zing MP3 API: {apiResponse.Msg}");
                    return await _artistRepository.GetAllAsync();
                }

                var zingArtist = apiResponse.Data.data;
                var existingArtists = await _artistRepository.GetAllAsync();

                var artist = existingArtists.FirstOrDefault(a => a.ZingMp3ArtistId == zingArtist.id);
                if (artist == null)
                {
                    artist = new Artist
                    {
                        Name = zingArtist.name,
                        NormalizedName = RemoveDiacritics(zingArtist.name),
                        Image = zingArtist.thumbnail,
                        Country = zingArtist.national,
                        Bio = zingArtist.bio ?? zingArtist.sortBiography,
                        ZingMp3ArtistId = zingArtist.id,
                        IsActive = true,
                        CreatedDate = DateTime.UtcNow,
                        UpdatedDate = DateTime.UtcNow
                    };
                    await _artistRepository.AddAsync(artist);
                }
                else
                {
                    artist.Name = zingArtist.name;
                    artist.NormalizedName = RemoveDiacritics(zingArtist.name);
                    artist.Image = zingArtist.thumbnail;
                    artist.Country = zingArtist.national;
                    artist.Bio = zingArtist.bio ?? zingArtist.sortBiography;
                    artist.UpdatedDate = DateTime.UtcNow;
                    await _artistRepository.UpdateAsync(artist);
                }

                return await _artistRepository.GetAllAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] General Error: {ex.Message}");
                return await _artistRepository.GetAllAsync(); // Trả về danh sách hiện có thay vì ném lỗi
            }
        }

        // Cập nhật lớp ánh xạ để phù hợp với cấu trúc JSON từ /api/artist-by-id/:id
        private class ZingMp3ApiResponse
        {
            public int Err { get; set; }
            public string Msg { get; set; }
            public ZingMp3ArtistData Data { get; set; }
            public long Timestamp { get; set; }
        }

        private class ZingMp3ArtistData
        {
            public int Err { get; set; }
            public string Msg { get; set; }
            public ZingMp3ArtistDetails data { get; set; }
        }

        private class ZingMp3ArtistDetails
        {
            public string id { get; set; }
            public string name { get; set; }
            public string link { get; set; }
            public string thumbnail { get; set; }
            public string thumbnailM { get; set; }
            public string national { get; set; }
            public string bio { get; set; }
            public string sortBiography { get; set; }
            public string birthday { get; set; }
            public string realname { get; set; }
            public int totalFollow { get; set; }
        }
    }
}
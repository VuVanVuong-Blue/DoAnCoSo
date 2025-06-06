using Microsoft.EntityFrameworkCore;
using System_Music.Models.SqlModels;
using System_Music.Repositories.Implementations;
using System_Music.Services.Interfaces;
using System.Net.Http;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using System.Threading.Tasks;
using System.Security.Cryptography;

namespace System_Music.Services.Repositories
{
    public class GenreRepository : Repository<Genre>, IGenreRepository
    {
        private readonly SmartMusicDbContext _context;
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;

        public GenreRepository(SmartMusicDbContext context, IHttpClientFactory httpClientFactory, IConfiguration configuration) : base(context)
        {
            _context = context;
            _httpClient = httpClientFactory.CreateClient();
            _configuration = configuration;
        }

        public async Task<List<Genre>> GetGenresByTrackIdAsync(int trackId)
        {
            return await _context.TrackGenres
                .Where(tg => tg.TrackId == trackId)
                .Include(tg => tg.Genre)
                .Select(tg => tg.Genre!)
                .ToListAsync();
        }

        public async Task<bool> GenreExistsAsync(int genreId)
        {
            return await _context.Genres.AnyAsync(g => g.GenreId == genreId);
        }

        public Task<Genre> GetByNameAsync(string name)
        {
            throw new NotImplementedException();
        }

        public async Task UpdateAsync(Genre genre)
        {
            Console.WriteLine($"[DEBUG REPO] Genre before update: Name={genre.Name}, Description={genre.Description}");

            _context.Genres.Update(genre);
            var result = await _context.SaveChangesAsync();

            Console.WriteLine($"[DEBUG] SaveChanges result: {result}");
        }

        public async Task<List<Genre>> SyncGenresFromZingMp3Async()
        {
            var baseUrl = _configuration["ZingMp3Api:BaseUrl"];
            var apiKey = _configuration["ZingMp3Api:ApiKey"];
            var secretKey = _configuration["ZingMp3Api:SecretKey"];
            var version = _configuration["ZingMp3Api:Version"];

            if (string.IsNullOrEmpty(baseUrl) || string.IsNullOrEmpty(apiKey) || string.IsNullOrEmpty(secretKey) || string.IsNullOrEmpty(version))
            {
                Console.WriteLine("[DEBUG] Missing configuration for ZingMp3Api.");
                return new List<Genre>();
            }

            try
            {
                var ctime = DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString();
                var endpoint = "/category/get-list";
                var sig = GenerateSignature(endpoint, "", ctime, secretKey);

                var url = $"{baseUrl}{endpoint}?ctime={ctime}&version={version}&sig={sig}&apiKey={apiKey}";
                Console.WriteLine($"[DEBUG] Calling Zing MP3 API: {url}");

                var response = await _httpClient.GetAsync(url);
                if (!response.IsSuccessStatusCode)
                {
                    Console.WriteLine($"[DEBUG] Failed to get genres list from Zing MP3: {response.StatusCode} - {response.ReasonPhrase}");
                    return new List<Genre>();
                }

                var json = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"[DEBUG] Raw Response from Zing MP3: {json}");
                var data = JsonDocument.Parse(json).RootElement;
                if (data.GetProperty("err").GetInt32() != 0)
                {
                    Console.WriteLine($"[DEBUG] Zing MP3 API error: {data.GetProperty("msg").GetString()}");
                    return new List<Genre>();
                }

                var genres = new List<Genre>();
                if (data.TryGetProperty("data", out var dataElement) && dataElement.TryGetProperty("genres", out var genreArray) && genreArray.ValueKind == JsonValueKind.Array)
                {
                    foreach (var g in genreArray.EnumerateArray())
                    {
                        if (g.TryGetProperty("name", out var genreNameElement) && g.TryGetProperty("id", out var zingMp3GenreIdElement))
                        {
                            var genreName = genreNameElement.GetString();
                            var zingMp3GenreId = zingMp3GenreIdElement.GetString();
                            if (string.IsNullOrEmpty(genreName)) continue;

                            Console.WriteLine($"[DEBUG] Processing genre: Name={genreName}, ZingMp3GenreId={zingMp3GenreId}");

                            var existingGenre = await _context.Genres.FirstOrDefaultAsync(g => g.Name == genreName);
                            if (existingGenre == null)
                            {
                                var newGenre = new Genre
                                {
                                    Name = genreName,
                                    Description = $"Thể loại từ Zing MP3: {genreName} (ID: {zingMp3GenreId})",
                                    ZingMp3GenreId = zingMp3GenreId,
               
                                };
                                await _context.Genres.AddAsync(newGenre);
                                genres.Add(newGenre);
                                Console.WriteLine($"[DEBUG] Added new genre: {genreName}");
                            }
                            else
                            {
                                existingGenre.Description = $"Thể loại từ Zing MP3: {genreName} (ID: {zingMp3GenreId})";
                                existingGenre.ZingMp3GenreId = zingMp3GenreId;
                      
                                _context.Genres.Update(existingGenre);
                                genres.Add(existingGenre);
                                Console.WriteLine($"[DEBUG] Updated existing genre: {genreName}");
                            }
                        }
                    }

                    var saveResult = await _context.SaveChangesAsync();
                    Console.WriteLine($"[DEBUG] SaveChanges result: {saveResult} genres saved.");
                }
                else
                {
                    Console.WriteLine("[DEBUG] No genres found in response or invalid data structure.");
                }

                return genres;
            }
            catch (JsonException ex)
            {
                Console.WriteLine($"[DEBUG] JSON parsing error: {ex.Message}");
                return new List<Genre>();
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"[DEBUG] HTTP request error: {ex.Message}");
                return new List<Genre>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[DEBUG] General error syncing genres: {ex.Message} - StackTrace: {ex.StackTrace}");
                return new List<Genre>();
            }
        }

        private string GenerateSignature(string endpoint, string songId, string ctime, string secretKey)
        {
            var path = endpoint.StartsWith("/") ? endpoint : $"/{endpoint}";
            var input = string.IsNullOrEmpty(songId) ? $"ctime={ctime}" : $"ctime={ctime}id={songId}";
            using (var hmac = new HMACSHA512(System.Text.Encoding.UTF8.GetBytes(secretKey)))
            {
                var hash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(path + input));
                return BitConverter.ToString(hash).Replace("-", "").ToLower();
            }
        }
    }
}
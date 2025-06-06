using System_Music.Models.SqlModels;
using System_Music.Services.Interfaces;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System.Net.Http;
using System.Threading.Tasks;

namespace System_Music.Services
{
    public class GenreService : IGenreService
    {
        private readonly IGenreRepository _genreRepository;
        private readonly HttpClient _httpClient;
        private readonly string _zingMp3ApiUrl;

        public GenreService(IGenreRepository genreRepository, HttpClient httpClient, IConfiguration configuration)
        {
            _genreRepository = genreRepository;
            _httpClient = httpClient;
            _zingMp3ApiUrl = configuration.GetSection("ZingMp3Api:BaseUrl").Value;
        }

        public async Task<List<Genre>> GetAllGenresAsync()
        {
            return await _genreRepository.GetAllAsync();
        }

        public async Task<Genre> GetGenreByIdAsync(int id)
        {
            var genre = await _genreRepository.GetByIdAsync(id);
            if (genre == null)
            {
                throw new KeyNotFoundException($"Genre with ID {id} not found.");
            }
            return genre;
        }

        public async Task AddGenreAsync(Genre genre)
        {
            if (genre == null)
            {
                throw new ArgumentNullException(nameof(genre));
            }

            // Kiểm tra xem tên thể loại đã tồn tại chưa
            var existingGenre = await _genreRepository.GetAsync(g => g.Name == genre.Name);
            if (existingGenre != null)
            {
                throw new InvalidOperationException($"Genre with name '{genre.Name}' already exists.");
            }

            await _genreRepository.AddAsync(genre);
        }

        public async Task UpdateGenreAsync(Genre genre)
        {
            if (genre == null)
                throw new ArgumentNullException(nameof(genre));

            var existingGenre = await _genreRepository.GetByIdAsync(genre.GenreId);
            if (existingGenre == null)
                throw new KeyNotFoundException($"Genre with ID {genre.GenreId} not found.");

            var genreWithSameName = await _genreRepository.GetAsync(g => g.Name == genre.Name && g.GenreId != genre.GenreId);
            if (genreWithSameName != null)
                throw new InvalidOperationException($"Genre with name '{genre.Name}' already exists.");

            // ✅ Update properties on the entity being tracked
            existingGenre.Name = genre.Name;
            existingGenre.Description = genre.Description;

            // ✅ Pass the already-tracked instance
            await _genreRepository.UpdateAsync(existingGenre);
        }

        public async Task DeleteGenreAsync(int id)
        {
            // Kiểm tra xem thể loại có tồn tại không
            var genre = await _genreRepository.GetByIdAsync(id);
            if (genre == null)
            {
                throw new KeyNotFoundException($"Genre with ID {id} not found.");
            }

            // Kiểm tra xem thể loại có đang được sử dụng bởi bài hát nào không
            var trackGenres = await _genreRepository.GetGenresByTrackIdAsync(id);
            if (trackGenres.Any())
            {
                throw new InvalidOperationException($"Cannot delete genre with ID {id} because it is associated with one or more tracks.");
            }

            await _genreRepository.DeleteAsync(id);
        }

        public async Task<List<Genre>> GetGenresByTrackIdAsync(int trackId)
        {
            return await _genreRepository.GetGenresByTrackIdAsync(trackId);
        }

        public async Task<bool> GenreExistsAsync(int genreId)
        {
            return await _genreRepository.GenreExistsAsync(genreId);
        }

        public async Task<List<Genre>> SyncGenresFromZingMp3Async()
        {
            try
            {
                // Gọi API từ server.js để lấy danh sách thể loại
                var response = await _httpClient.GetAsync($"{_zingMp3ApiUrl}/api/genres");
                response.EnsureSuccessStatusCode();

                var jsonString = await response.Content.ReadAsStringAsync();
                var apiResponse = JsonConvert.DeserializeObject<ZingMp3ApiResponse>(jsonString);

                if (apiResponse.Err != 0 || apiResponse.Data == null || apiResponse.Data.Genres == null)
                {
                    throw new Exception($"Error from Zing MP3 API: {apiResponse.Msg}");
                }

                var zingGenres = apiResponse.Data.Genres;
                var existingGenres = await _genreRepository.GetAllAsync();

                // Đồng bộ danh sách thể loại
                foreach (var zingGenre in zingGenres)
                {
                    var existingGenre = existingGenres.FirstOrDefault(g => g.ZingMp3GenreId == zingGenre.Id);
                    if (existingGenre == null)
                    {
                        // Thêm mới nếu chưa tồn tại
                        var newGenre = new Genre
                        {
                            Name = zingGenre.Name,
                            Description = zingGenre.Title,
                            ZingMp3GenreId = zingGenre.Id
                        };
                        await _genreRepository.AddAsync(newGenre);
                    }
                    else
                    {
                        // Cập nhật nếu đã tồn tại
                        existingGenre.Name = zingGenre.Name;
                        existingGenre.Description = zingGenre.Title;
                        await _genreRepository.UpdateAsync(existingGenre);
                    }
                }

                // Trả về danh sách thể loại sau khi đồng bộ
                return await _genreRepository.GetAllAsync();
            }
            catch (HttpRequestException ex)
            {
                throw new Exception($"Failed to fetch genres from Zing MP3 API: {ex.Message}", ex);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error syncing genres: {ex.Message}", ex);
            }
        }

        // Lớp ánh xạ dữ liệu từ API Zing MP3
        private class ZingMp3ApiResponse
        {
            public int Err { get; set; }
            public string Msg { get; set; }
            public ZingMp3Data Data { get; set; }
            public long Timestamp { get; set; }
        }

        private class ZingMp3Data
        {
            public List<ZingMp3Genre> Genres { get; set; }
        }

        private class ZingMp3Genre
        {
            public string Id { get; set; }
            public string Name { get; set; }
            public string Title { get; set; }
            public string Alias { get; set; }
            public string Link { get; set; }
        }
    }
}
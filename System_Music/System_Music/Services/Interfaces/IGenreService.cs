using System_Music.Models.SqlModels;

namespace System_Music.Services.Interfaces
{
    public interface IGenreService
    {
        Task<List<Genre>> GetAllGenresAsync();
        Task<Genre> GetGenreByIdAsync(int id);
        Task AddGenreAsync(Genre genre);
        Task UpdateGenreAsync(Genre genre);
        Task DeleteGenreAsync(int id);
        Task<List<Genre>> GetGenresByTrackIdAsync(int trackId);
        Task<bool> GenreExistsAsync(int genreId);
        Task<List<Genre>> SyncGenresFromZingMp3Async(); // Thêm phương thức mới
    }
}   
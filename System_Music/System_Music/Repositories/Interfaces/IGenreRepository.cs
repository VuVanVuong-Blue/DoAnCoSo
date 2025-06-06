using System_Music.Models.SqlModels;
using System_Music.Repositories.Interfaces;

namespace System_Music.Services.Interfaces
{
    public interface IGenreRepository : IRepository<Genre>
    {
        Task<List<Genre>> GetGenresByTrackIdAsync(int trackId);
        Task<bool> GenreExistsAsync(int genreId);
        Task<Genre> GetByNameAsync(string name);
        Task UpdateAsync(Genre genre);
        Task<List<Genre>> SyncGenresFromZingMp3Async(); // Thêm phương thức mới

    }
}
using System_Music.Models.SqlModels;

namespace System_Music.Repositories.Interfaces
{
    public interface ITrackGenreRepository
    {
        Task<List<TrackGenre>> GetAllAsync();
        Task<TrackGenre> GetByIdAsync(int trackId, int genreId);
        Task AddAsync(TrackGenre trackGenre);
        Task DeleteAsync(int trackId, int genreId);
        Task<List<TrackGenre>> GetGenresByTrackAsync(int trackId);
        Task<List<TrackGenre>> GetTracksByGenreAsync(int genreId);
        Task<Track> GetByIdWithDetailsAsync(int id);
    }
}
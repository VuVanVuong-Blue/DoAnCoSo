using System_Music.Models.SqlModels;

namespace System_Music.Services.Interfaces
{
    public interface ITrackGenreService
    {
        Task<List<TrackGenre>> GetAllTrackGenresAsync();
        Task<TrackGenre> GetTrackGenreByIdAsync(int trackId, int genreId);
        Task AddTrackGenreAsync(TrackGenre trackGenre);
        Task DeleteTrackGenreAsync(int trackId, int genreId);
        Task<List<TrackGenre>> GetGenresByTrackAsync(int trackId);
        Task<List<TrackGenre>> GetTracksByGenreAsync(int genreId);
    }
}
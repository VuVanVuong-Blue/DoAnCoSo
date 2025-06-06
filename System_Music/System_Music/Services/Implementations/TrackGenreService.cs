using System_Music.Models.SqlModels;
using System_Music.Repositories.Interfaces;
using System_Music.Services.Interfaces;

namespace System_Music.Services.Implementations
{
    public class TrackGenreService : ITrackGenreService
    {
        private readonly ITrackGenreRepository _trackGenreRepository;

        public TrackGenreService(ITrackGenreRepository trackGenreRepository)
        {
            _trackGenreRepository = trackGenreRepository;
        }

        public async Task<List<TrackGenre>> GetAllTrackGenresAsync()
        {
            return await _trackGenreRepository.GetAllAsync();
        }

        public async Task<TrackGenre> GetTrackGenreByIdAsync(int trackId, int genreId)
        {
            return await _trackGenreRepository.GetByIdAsync(trackId, genreId);
        }

        public async Task AddTrackGenreAsync(TrackGenre trackGenre)
        {
            await _trackGenreRepository.AddAsync(trackGenre);
        }

        public async Task DeleteTrackGenreAsync(int trackId, int genreId)
        {
            await _trackGenreRepository.DeleteAsync(trackId, genreId);
        }

        public async Task<List<TrackGenre>> GetGenresByTrackAsync(int trackId)
        {
            return await _trackGenreRepository.GetGenresByTrackAsync(trackId);
        }

        public async Task<List<TrackGenre>> GetTracksByGenreAsync(int genreId)
        {
            return await _trackGenreRepository.GetTracksByGenreAsync(genreId);
        }
    }
}
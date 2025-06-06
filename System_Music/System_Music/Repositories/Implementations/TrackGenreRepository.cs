using Microsoft.EntityFrameworkCore;
using System_Music.Models.SqlModels;
using System_Music.Repositories.Interfaces;

namespace System_Music.Repositories.Implementations
{
    public class TrackGenreRepository : ITrackGenreRepository
    {
        private readonly SmartMusicDbContext _context;

        public TrackGenreRepository(SmartMusicDbContext context)
        {
            _context = context;
        }

        public async Task<List<TrackGenre>> GetAllAsync()
        {
            return await _context.TrackGenres
                .Include(tg => tg.Track)
                .Include(tg => tg.Genre)
                .ToListAsync();
        }

        public async Task<TrackGenre> GetByIdAsync(int trackId, int genreId)
        {
            return await _context.TrackGenres
                .Include(tg => tg.Track)
                .Include(tg => tg.Genre)
                .FirstOrDefaultAsync(tg => tg.TrackId == trackId && tg.GenreId == genreId);
        }

        public async Task AddAsync(TrackGenre trackGenre)
        {
            await _context.TrackGenres.AddAsync(trackGenre);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int trackId, int genreId)
        {
            var trackGenre = await _context.TrackGenres
                .FirstOrDefaultAsync(tg => tg.TrackId == trackId && tg.GenreId == genreId);
            if (trackGenre != null)
            {
                _context.TrackGenres.Remove(trackGenre);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<List<TrackGenre>> GetGenresByTrackAsync(int trackId)
        {
            return await _context.TrackGenres
                .Where(tg => tg.TrackId == trackId)
                .Include(tg => tg.Genre)
                .ToListAsync();
        }

        public async Task<List<TrackGenre>> GetTracksByGenreAsync(int genreId)
        {
            return await _context.TrackGenres
                .Where(tg => tg.GenreId == genreId)
                .Include(tg => tg.Track)
                .ToListAsync();
        }

        public Task<Track> GetByIdWithDetailsAsync(int id)
        {
            return _context.Tracks
                .Include(t => t.TrackGenres)
                    .ThenInclude(tg => tg.Genre)
                .Include(t => t.TrackArtists)
                    .ThenInclude(ta => ta.Artist)
                .FirstOrDefaultAsync(t => t.TrackId == id);
        }
    }
}
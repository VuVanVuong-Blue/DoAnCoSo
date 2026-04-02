using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System_Music.Models.SqlModels;
using System_Music.Repositories.Interfaces;

namespace System_Music.Repositories.Implementations
{
    public class TrackArtistRepository : ITrackArtistRepository
    {
        private readonly SmartMusicDbContext _context;

        public TrackArtistRepository(SmartMusicDbContext context)
        {
            _context = context;
        }

        public async Task<ICollection<TrackArtist>> GetTrackArtistsByTrackIdAsync(int trackId)
        {
            return await _context.TrackArtists
                .Include(ta => ta.Artist)
                .Where(ta => ta.TrackId == trackId)
                .ToListAsync();
        }

        public async Task<List<TrackArtist>> GetAllAsync()
        {
            return await _context.TrackArtists.ToListAsync();
        }

        public async Task<TrackArtist> GetByIdAsync(int trackId, int artistId)
        {
            return await _context.TrackArtists.FindAsync(trackId, artistId);
        }

        public async Task AddAsync(TrackArtist trackArtist)
        {
            await _context.TrackArtists.AddAsync(trackArtist);
        }

        public async Task DeleteAsync(int trackId, int artistId)
        {
            var trackArtist = await GetByIdAsync(trackId, artistId);
            if (trackArtist != null)
            {
                _context.TrackArtists.Remove(trackArtist);
            }
        }

        public async Task<List<TrackArtist>> GetArtistsByTrackAsync(int trackId)
        {
            return await _context.TrackArtists
                .Include(ta => ta.Artist)
                .Where(ta => ta.TrackId == trackId)
                .ToListAsync();
        }

        public async Task<List<TrackArtist>> GetTracksByArtistAsync(int artistId)
        {
            return await _context.TrackArtists
                .Include(ta => ta.Track)
                .Where(ta => ta.ArtistId == artistId)
                .ToListAsync();
        }
    }
}
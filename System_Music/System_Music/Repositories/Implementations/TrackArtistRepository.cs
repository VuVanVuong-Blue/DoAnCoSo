using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;
using System_Music.Models.SqlModels;
using System_Music.Services.Interfaces;

namespace System_Music.Services.Implementations
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
    }
}
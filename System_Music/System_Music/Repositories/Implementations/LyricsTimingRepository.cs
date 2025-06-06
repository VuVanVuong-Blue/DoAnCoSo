using Microsoft.EntityFrameworkCore;
using System_Music.Models.SqlModels;
using System_Music.Repositories.Interfaces;

namespace System_Music.Repositories.Implementations
{
    public class LyricsTimingRepository : Repository<LyricsTiming>, ILyricsTimingRepository
    {
        public LyricsTimingRepository(SmartMusicDbContext context) : base(context)
        {
        }

        public async Task<List<LyricsTiming>> GetLyricsByTrackAsync(int trackId)
        {
            return await _context.LyricsTimings
                .Where(lt => lt.TrackId == trackId)
                .ToListAsync();
        }

        public override async Task<List<LyricsTiming>> GetAllAsync()
        {
            return await _context.LyricsTimings
                .Include(lt => lt.Track)
                .ToListAsync();
        }

        public  async Task<LyricsTiming> GetByIdAsync(object id)
        {
            return await _context.LyricsTimings
                .Include(lt => lt.Track)
                .FirstOrDefaultAsync(lt => lt.LyricsTimingId == (int)id);
        }
    }
}
using Microsoft.EntityFrameworkCore;
using System_Music.Models.SqlModels;
using System_Music.Repositories.Interfaces;

namespace System_Music.Repositories.Implementations
{
    public class DownloadRepository : Repository<Download>, IDownloadRepository
    {
        public DownloadRepository(SmartMusicDbContext context) : base(context)
        {
        }

        public async Task<List<Download>> GetDownloadsByUserAsync(string userId)
        {
            return await _context.Downloads
                .Where(d => d.UserId == userId)
                .Include(d => d.Track)
                .ToListAsync();
        }

        public async Task<List<Download>> GetDownloadsByTrackAsync(int trackId)
        {
            return await _context.Downloads
                .Where(d => d.TrackId == trackId)
                .Include(d => d.User)
                .ToListAsync();
        }

        public override async Task<List<Download>> GetAllAsync()
        {
            return await _context.Downloads
                .Include(d => d.Track)
                .Include(d => d.User)
                .ToListAsync();
        }

        public async Task<Download> GetByIdAsync(object id)
        {
            return await _context.Downloads
                .Include(d => d.Track)
                .Include(d => d.User)
                .FirstOrDefaultAsync(d => d.DownloadId == (int)id);
        }
    }
}
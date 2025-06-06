using Microsoft.EntityFrameworkCore;
using System_Music.Models.SqlModels;
using System_Music.Repositories.Interfaces;

namespace System_Music.Repositories.Implementations
{
    public class PlayQueueRepository : Repository<PlayQueue>, IPlayQueueRepository
    {
        public PlayQueueRepository(SmartMusicDbContext context) : base(context)
        {
        }

        public async Task<List<PlayQueue>> GetQueueByUserAsync(string userId)
        {
            return await _context.PlayQueues
                .Where(pq => pq.UserId == userId)
                .Include(pq => pq.Track)
                .ToListAsync();
        }

        public override async Task<List<PlayQueue>> GetAllAsync()
        {
            return await _context.PlayQueues
                .Include(pq => pq.Track)
                .Include(pq => pq.User)
                .ToListAsync();
        }

        public async Task<PlayQueue> GetByIdAsync(object id)
        {
            return await _context.PlayQueues
                .Include(pq => pq.Track)
                .Include(pq => pq.User)
                .FirstOrDefaultAsync(pq => pq.QueueId == (int)id);
        }
    }
}
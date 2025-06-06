using Microsoft.EntityFrameworkCore;
using System_Music.Models.SqlModels;
using System_Music.Repositories.Interfaces;

namespace System_Music.Repositories.Implementations
{
    public class ChartInteractionRepository : Repository<ChartInteraction>, IChartInteractionRepository
    {
        public ChartInteractionRepository(SmartMusicDbContext context) : base(context)
        {
        }

        public async Task<List<ChartInteraction>> GetInteractionsByUserAsync(string userId)
        {
            return await _context.ChartInteractions
                .Where(ci => ci.UserId == userId)
                .ToListAsync();
        }

        public async Task<List<ChartInteraction>> GetInteractionsByTrackAsync(int trackId)
        {
            return await _context.ChartInteractions
                .Where(ci => ci.TrackId == trackId)
                .ToListAsync();
        }
    }
}
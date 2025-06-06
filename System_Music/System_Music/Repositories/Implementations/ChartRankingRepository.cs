using Microsoft.EntityFrameworkCore;
using System_Music.Models.SqlModels;
using System_Music.Repositories.Interfaces;
using System_Music.Services.Repositories;

namespace System_Music.Repositories.Implementations
{
    public class ChartRankingRepository : Repository<ChartRanking>, IChartRankingRepository
    {
        public ChartRankingRepository(SmartMusicDbContext context) : base(context)
        {
        }

        public async Task<List<ChartRanking>> GetRankingsByTrackAsync(int trackId)
        {
            return await _context.ChartRankings
                .Where(cr => cr.TrackId == trackId)
                .Include(cr => cr.Track)
                .ToListAsync();
        }

        public async Task<List<ChartRanking>> GetRankingsByCountryAsync(string country)
        {
            return await _context.ChartRankings
                .Where(cr => cr.Country == country)
                .Include(cr => cr.Track)
                .ToListAsync();
        }

        public override async Task<List<ChartRanking>> GetAllAsync()
        {
            return await _context.ChartRankings
                .Include(cr => cr.Track)
                .ToListAsync();
        }


    }
}
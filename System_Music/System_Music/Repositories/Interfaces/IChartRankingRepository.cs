using System_Music.Models.SqlModels;

namespace System_Music.Repositories.Interfaces
{
    public interface IChartRankingRepository : IRepository<ChartRanking>
    {
        Task<List<ChartRanking>> GetRankingsByTrackAsync(int trackId);
        Task<List<ChartRanking>> GetRankingsByCountryAsync(string country);
    }
}
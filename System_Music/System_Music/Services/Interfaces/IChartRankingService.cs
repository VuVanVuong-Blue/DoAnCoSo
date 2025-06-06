using System_Music.Models.SqlModels;

namespace System_Music.Services.Interfaces
{
    public interface IChartRankingService
    {
        Task<List<ChartRanking>> GetTopTracksAsync(string country, string timeFrame, int limit = 50);
        Task UpdateChartRankingAsync(string country, string timeFrame);
    }
}
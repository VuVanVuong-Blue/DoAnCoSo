using System_Music.Models.DTOs;

namespace System_Music.Services.Interfaces
{
    public interface IChartRankingService
    {
        Task<List<ChartRankingDto>> GetTopTracksAsync(string country, string timeFrame, int limit = 50);
        Task UpdateChartRankingAsync(string country, string timeFrame);
    }
}
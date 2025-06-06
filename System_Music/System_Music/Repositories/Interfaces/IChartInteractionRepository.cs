using System_Music.Models.SqlModels;

namespace System_Music.Repositories.Interfaces
{
    public interface IChartInteractionRepository : IRepository<ChartInteraction>
    {
        Task<List<ChartInteraction>> GetInteractionsByUserAsync(string userId);
        Task<List<ChartInteraction>> GetInteractionsByTrackAsync(int trackId);
    }
}
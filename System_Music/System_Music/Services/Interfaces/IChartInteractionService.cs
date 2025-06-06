using System_Music.Models.SqlModels;

namespace System_Music.Services.Interfaces
{
    public interface IChartInteractionService
    {
        Task<List<ChartInteraction>> GetAllInteractionsAsync();
        Task<ChartInteraction> GetInteractionByIdAsync(int id);
        Task AddInteractionAsync(ChartInteraction interaction);
        Task DeleteInteractionAsync(int id);
        Task<List<ChartInteraction>> GetInteractionsByUserAsync(string userId);
        Task<List<ChartInteraction>> GetInteractionsByTrackAsync(int trackId);
        Task<int> GetInteractionCountByTrackAsync(int trackId);
    }
}
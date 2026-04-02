using System_Music.Models.SqlModels;
using System_Music.Models.DTOs;

namespace System_Music.Services.Interfaces
{
    public interface IListenHistoryService
    {
        Task<List<ListenHistoryDto>> GetAllListenHistoriesAsync();
        Task<List<ListenHistoryDto>> GetListenHistoriesByUserAsync(string userId);
        Task<List<ListenHistoryDto>> GetListenHistoriesByTrackAsync(int trackId);
        Task AddListenHistoryAsync(ListenHistory listenHistory);
        Task<bool> HasListenedAsync(string userId, EntityType entityType, int entityId);
    }
}
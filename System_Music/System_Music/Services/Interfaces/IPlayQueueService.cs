using System_Music.Models.SqlModels;

namespace System_Music.Services.Interfaces
{
    public interface IPlayQueueService
    {
        Task<List<PlayQueue>> GetAllQueuesAsync();
        Task<PlayQueue> GetQueueByIdAsync(int id);
        Task AddQueueAsync(PlayQueue playQueue);
        Task DeleteQueueAsync(int id);
        Task<List<PlayQueue>> GetQueueByUserAsync(string userId);
        Task ClearQueueByUserAsync(string userId);
    }
}
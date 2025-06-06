using System_Music.Models.SqlModels;
using System_Music.Repositories.Interfaces;
using System_Music.Services.Interfaces;

namespace System_Music.Services.Implementations
{
    public class PlayQueueService : IPlayQueueService
    {
        private readonly IPlayQueueRepository _playQueueRepository;

        public PlayQueueService(IPlayQueueRepository playQueueRepository)
        {
            _playQueueRepository = playQueueRepository;
        }

        public async Task<List<PlayQueue>> GetAllQueuesAsync()
        {
            return await _playQueueRepository.GetAllAsync();
        }

        public async Task<PlayQueue> GetQueueByIdAsync(int id)
        {
            return await _playQueueRepository.GetByIdAsync(id);
        }

        public async Task AddQueueAsync(PlayQueue playQueue)
        {
            await _playQueueRepository.AddAsync(playQueue);
        }

        public async Task DeleteQueueAsync(int id)
        {
            await _playQueueRepository.DeleteAsync(id);
        }

        public async Task<List<PlayQueue>> GetQueueByUserAsync(string userId)
        {
            return await _playQueueRepository.GetQueueByUserAsync(userId);
        }

        public async Task ClearQueueByUserAsync(string userId)
        {
            var queues = await _playQueueRepository.GetQueueByUserAsync(userId);
            foreach (var queue in queues)
            {
                await _playQueueRepository.DeleteAsync(queue.QueueId);
            }
        }
    }
}
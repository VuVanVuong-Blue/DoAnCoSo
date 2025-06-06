using System_Music.Models.SqlModels;

namespace System_Music.Repositories.Interfaces
{
    public interface IPlayQueueRepository : IRepository<PlayQueue>
    {
        Task<List<PlayQueue>> GetQueueByUserAsync(string userId);
    }
}
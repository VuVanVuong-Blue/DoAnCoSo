using System_Music.Models.SqlModels;

namespace System_Music.Repositories.Interfaces
{
    public interface IDownloadRepository : IRepository<Download>
    {
        Task<List<Download>> GetDownloadsByUserAsync(string userId);
        Task<List<Download>> GetDownloadsByTrackAsync(int trackId);
    }
}
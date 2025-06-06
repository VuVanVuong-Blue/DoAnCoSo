using System_Music.Models.SqlModels;

namespace System_Music.Services.Interfaces
{
    public interface IDownloadService
    {
        Task<List<Download>> GetAllDownloadsAsync();
        Task<Download> GetDownloadByIdAsync(int id);
        Task AddDownloadAsync(Download download);
        Task DeleteDownloadAsync(int id);
        Task<List<Download>> GetDownloadsByUserAsync(string userId);
        Task<List<Download>> GetDownloadsByTrackAsync(int trackId);
        Task<int> GetDownloadCountByTrackAsync(int trackId);
    }
}
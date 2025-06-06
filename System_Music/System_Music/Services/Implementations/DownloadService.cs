using System_Music.Models.SqlModels;
using System_Music.Repositories.Interfaces;
using System_Music.Services.Interfaces;

namespace System_Music.Services.Implementations
{
    public class DownloadService : IDownloadService
    {
        private readonly IDownloadRepository _downloadRepository;

        public DownloadService(IDownloadRepository downloadRepository)
        {
            _downloadRepository = downloadRepository;
        }

        public async Task<List<Download>> GetAllDownloadsAsync()
        {
            return await _downloadRepository.GetAllAsync();
        }

        public async Task<Download> GetDownloadByIdAsync(int id)
        {
            return await _downloadRepository.GetByIdAsync(id);
        }

        public async Task AddDownloadAsync(Download download)
        {
            await _downloadRepository.AddAsync(download);
        }

        public async Task DeleteDownloadAsync(int id)
        {
            await _downloadRepository.DeleteAsync(id);
        }

        public async Task<List<Download>> GetDownloadsByUserAsync(string userId)
        {
            return await _downloadRepository.GetDownloadsByUserAsync(userId);
        }

        public async Task<List<Download>> GetDownloadsByTrackAsync(int trackId)
        {
            return await _downloadRepository.GetDownloadsByTrackAsync(trackId);
        }

        public async Task<int> GetDownloadCountByTrackAsync(int trackId)
        {
            var downloads = await _downloadRepository.GetDownloadsByTrackAsync(trackId);
            return downloads.Count;
        }
    }
}
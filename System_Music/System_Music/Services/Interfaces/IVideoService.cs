using System_Music.Models.SqlModels;

namespace System_Music.Interfaces
{
    public interface IVideoService
    {
        Task<Video> GetVideoByIdAsync(string encodeId);
        Task<IEnumerable<Video>> GetAllVideosAsync();
        Task AddVideoAsync(Video video);
        Task UpdateVideoAsync(Video video);
        Task DeleteVideoAsync(string encodeId);
        Task<IEnumerable<Video>> GetRelatedVideosAsync(string videoId);
    }
}
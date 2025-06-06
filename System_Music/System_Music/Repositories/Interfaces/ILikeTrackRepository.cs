using System_Music.Models.SqlModels;

namespace System_Music.Repositories.Interfaces
{
    public interface ILikeTrackRepository : IRepository<LikeTrack>
    {
        Task<int> GetLikeCountAsync(int trackId);
        Task<List<LikeTrack>> GetLikesByTrackAsync(int trackId);
        Task<List<LikeTrack>> GetLikesByUserAsync(string userId);
        Task<bool> HasUserLikedTrackAsync(string userId, int trackId); // Sửa từ int sang string
    }
}
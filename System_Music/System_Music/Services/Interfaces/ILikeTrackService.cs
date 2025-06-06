using System_Music.Models.SqlModels;

namespace System_Music.Services.Interfaces
{
    public interface ILikeTrackService
    {
        Task<LikeTrack> GetLikeByUserAndTrackAsync(string userId, int trackId);
        Task<List<LikeTrack>> GetAllLikesAsync();
        Task<LikeTrack> GetLikeByIdAsync(int id);
        Task AddLikeAsync(LikeTrack likeTrack);
        Task DeleteLikeAsync(int id);
        Task<List<LikeTrack>> GetLikesByUserAsync(string userId);
        Task<List<LikeTrack>> GetLikesByTrackAsync(int trackId);
        Task<bool> HasLikedTrackAsync(string userId, int trackId);
    }
}
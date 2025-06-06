using System_Music.Models.SqlModels;

namespace System_Music.Services.Interfaces
{
    public interface IFollowerService
    {
        Task<List<Follower>> GetAllFollowersAsync();
        Task<Follower> GetFollowerByIdAsync(int id);
        Task AddFollowerAsync(Follower follower);
        Task DeleteFollowerAsync(int id);
        Task<List<Follower>> GetFollowersByUserAsync(string userId);
        Task<List<Follower>> GetFollowersByArtistAsync(int artistId);
        Task<bool> IsFollowingAsync(string userId, int artistId);
    }
}
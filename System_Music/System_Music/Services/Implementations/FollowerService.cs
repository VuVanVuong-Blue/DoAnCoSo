using System_Music.Models.SqlModels;
using System_Music.Repositories.Interfaces;
using System_Music.Services.Interfaces;

namespace System_Music.Services.Implementations
{
    public class FollowerService : IFollowerService
    {
        private readonly IFollowerRepository _followerRepository;

        public FollowerService(IFollowerRepository followerRepository)
        {
            _followerRepository = followerRepository;
        }

        public async Task<List<Follower>> GetAllFollowersAsync()
        {
            return await _followerRepository.GetAllAsync();
        }

        public async Task<Follower> GetFollowerByIdAsync(int id)
        {
            return await _followerRepository.GetByIdAsync(id);
        }

        public async Task AddFollowerAsync(Follower follower)
        {
            await _followerRepository.AddAsync(follower);
        }

        public async Task DeleteFollowerAsync(int id)
        {
            await _followerRepository.DeleteAsync(id);
        }

        public async Task<List<Follower>> GetFollowersByUserAsync(string userId)
        {
            return await _followerRepository.GetFollowersByUserAsync(userId);
        }

        public async Task<List<Follower>> GetFollowersByArtistAsync(int artistId)
        {
            return await _followerRepository.GetFollowersByArtistAsync(artistId);
        }

        public async Task<bool> IsFollowingAsync(string userId, int artistId)
        {
            var followers = await _followerRepository.GetFollowersByUserAsync(userId);
            return followers.Any(f => f.ArtistId == artistId);
        }
    }
}
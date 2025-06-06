using System_Music.Models.SqlModels;

namespace System_Music.Repositories.Interfaces
{
    public interface IFollowerRepository : IRepository<Follower>
    {
        Task<List<Follower>> GetFollowersByUserAsync(string userId);
        Task<List<Follower>> GetFollowersByArtistAsync(int artistId);
        Task<Follower> GetByIdAsync(int id);
    }
}
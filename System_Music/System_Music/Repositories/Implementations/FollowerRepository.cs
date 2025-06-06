using Microsoft.EntityFrameworkCore;
using System_Music.Models.SqlModels;
using System_Music.Repositories.Interfaces;
using System_Music.Services.Repositories;

namespace System_Music.Repositories.Implementations
{
    public class FollowerRepository : Repository<Follower>, IFollowerRepository
    {
        public FollowerRepository(SmartMusicDbContext context) : base(context)
        {
        }

        public async Task<List<Follower>> GetFollowersByUserAsync(string userId)
        {
            return await _context.Followers
                .Where(f => f.UserId == userId)
                .Include(f => f.Artist)
                .ToListAsync();
        }

        public async Task<List<Follower>> GetFollowersByArtistAsync(int artistId)
        {
            return await _context.Followers
                .Where(f => f.ArtistId == artistId)
                .Include(f => f.User)
                .ToListAsync();
        }

        public override async Task<List<Follower>> GetAllAsync()
        {
            return await _context.Followers
                .Include(f => f.Artist)
                .Include(f => f.User)
                .ToListAsync();
        }
        public async Task<Follower> GetByIdAsync(int id)
        {
            return await _context.Followers
                .Include(f => f.Artist)
                .Include(f => f.User)
                .FirstOrDefaultAsync(f => f.FollowId == id);
        }
    }
}
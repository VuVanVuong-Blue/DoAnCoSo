using Microsoft.EntityFrameworkCore;
using System_Music.Models.SqlModels;
using System_Music.Repositories.Interfaces;

namespace System_Music.Repositories.Implementations
{
    public class UserMediaRepository : Repository<UserMedia>, IUserMediaRepository
    {
        private readonly SmartMusicDbContext _context;

        public UserMediaRepository(SmartMusicDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<List<UserMedia>> GetMediaByUserAsync(string userId)
        {
            return await _context.UserMedias
                .Where(um => um.UserId == userId)
                .Include(um => um.User)
                .Include(um => um.Playlist)
                .ToListAsync();
        }

        public async Task<List<UserMedia>> GetAllAsync()
        {
            return await _context.UserMedias
                .Include(um => um.User)
                .Include(um => um.Playlist)
                .ToListAsync();
        }

        public async Task<UserMedia> GetByIdAsync(int mediaId)
        {
            return await _context.UserMedias
                .Include(um => um.User)
                .Include(um => um.Playlist)
                .FirstOrDefaultAsync(um => um.MediaId == mediaId);
        }

        public async Task AddAsync(UserMedia userMedia)
        {
            await _context.UserMedias.AddAsync(userMedia);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int mediaId)
        {
            var media = await _context.UserMedias.FindAsync(mediaId);
            if (media != null)
            {
                _context.UserMedias.Remove(media);
                await _context.SaveChangesAsync();
            }
        }
    }
}
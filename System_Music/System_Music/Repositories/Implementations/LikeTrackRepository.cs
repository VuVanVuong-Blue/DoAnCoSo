using Microsoft.EntityFrameworkCore;
using System_Music.Models.SqlModels;
using System_Music.Repositories.Interfaces;

namespace System_Music.Repositories.Implementations
{
    public class LikeTrackRepository : Repository<LikeTrack>, ILikeTrackRepository
    {
        private readonly SmartMusicDbContext _context;

        public LikeTrackRepository(SmartMusicDbContext context) : base(context)
        {
            _context = context;
        }

        public override async Task<List<LikeTrack>> GetAllAsync()
        {
            return await _context.LikeTracks
                .Include(lt => lt.Track)
                .ToListAsync();
        }

        public async Task<LikeTrack> GetByIdAsync(int id)
        {
            return await _context.LikeTracks
                .Include(lt => lt.Track)
                .FirstOrDefaultAsync(lt => lt.Id == id);
        }

        public async Task<int> GetLikeCountAsync(int trackId)
        {
            return await _context.LikeTracks
                .CountAsync(lt => lt.TrackId == trackId);
        }

        public async Task<List<LikeTrack>> GetLikesByTrackAsync(int trackId)
        {
            return await _context.LikeTracks
                .Where(lt => lt.TrackId == trackId)
                .Include(lt => lt.User)
                .ToListAsync();
        }

        public async Task<List<LikeTrack>> GetLikesByUserAsync(string userId)
        {
            return await _context.LikeTracks
                .Where(lt => lt.UserId == userId)
                .Include(lt => lt.Track)
                .ToListAsync();
        }

        public async Task<bool> HasUserLikedTrackAsync(string userId, int trackId)
        {
            return await _context.LikeTracks
                .AnyAsync(lt => lt.UserId == userId && lt.TrackId == trackId);
        }
    }
}
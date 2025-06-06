using Microsoft.EntityFrameworkCore;
using System_Music.Models.SqlModels;
using System_Music.Repositories.Interfaces;
using System_Music.Services.Repositories;

namespace System_Music.Repositories.Implementations
{
    public class BlockListRepository : Repository<BlockList>, IBlockListRepository
    {
        public BlockListRepository(SmartMusicDbContext context) : base(context)
        {
        }

        public async Task<List<BlockList>> GetBlocksByUserAsync(string userId)
        {
            return await _context.BlockList
                .Where(b => b.UserId == userId)
                .Include(b => b.Artist)
                .Include(b => b.Track)
                .ToListAsync();
        }

        public override async Task<List<BlockList>> GetAllAsync()
        {
            return await _context.BlockList
                .Include(b => b.Artist)
                .Include(b => b.Track)
                .ToListAsync();
        }


    }
}
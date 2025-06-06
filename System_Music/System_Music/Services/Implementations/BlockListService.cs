using System_Music.Models.SqlModels;
using System_Music.Repositories.Interfaces;
using System_Music.Services.Interfaces;

namespace System_Music.Services.Implementations
{
    public class BlockListService : IBlockListService
    {
        private readonly IBlockListRepository _blockListRepository;

        public BlockListService(IBlockListRepository blockListRepository)
        {
            _blockListRepository = blockListRepository;
        }

        public async Task<List<BlockList>> GetAllBlocksAsync()
        {
            return await _blockListRepository.GetAllAsync();
        }

        public async Task<BlockList> GetBlockByIdAsync(int id)
        {
            return await _blockListRepository.GetByIdAsync(id);
        }

        public async Task AddBlockAsync(BlockList blockList)
        {
            await _blockListRepository.AddAsync(blockList);
        }

        public async Task DeleteBlockAsync(int id)
        {
            await _blockListRepository.DeleteAsync(id);
        }

        public async Task<List<BlockList>> GetBlocksByUserAsync(string userId)
        {
            return await _blockListRepository.GetBlocksByUserAsync(userId);
        }

        public async Task<bool> IsBlockedAsync(string userId, int? artistId, int? trackId)
        {
            var blocks = await _blockListRepository.GetBlocksByUserAsync(userId);
            return blocks.Any(b => (artistId.HasValue && b.ArtistId == artistId) ||
                                   (trackId.HasValue && b.TrackId == trackId));
        }
    }
}
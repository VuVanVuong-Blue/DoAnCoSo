using System_Music.Models.SqlModels;

namespace System_Music.Services.Interfaces
{
    public interface IBlockListService
    {
        Task<List<BlockList>> GetAllBlocksAsync();
        Task<BlockList> GetBlockByIdAsync(int id);
        Task AddBlockAsync(BlockList blockList);
        Task DeleteBlockAsync(int id);
        Task<List<BlockList>> GetBlocksByUserAsync(string userId);
        Task<bool> IsBlockedAsync(string userId, int? artistId, int? trackId);
    }
}
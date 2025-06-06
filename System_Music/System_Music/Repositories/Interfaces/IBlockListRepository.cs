using System_Music.Models.SqlModels;

namespace System_Music.Repositories.Interfaces
{
    public interface IBlockListRepository : IRepository<BlockList>
    {
        Task<List<BlockList>> GetBlocksByUserAsync(string userId);
    }
}
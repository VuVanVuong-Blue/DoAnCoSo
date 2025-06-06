using System_Music.Models.SqlModels;
using System.Threading.Tasks;

namespace System_Music.Repositories.Interfaces
{
    public interface IPlaylistRepository : IRepository<Playlist>
    {
        Task<Playlist> GetByIdAsync(int id);
        Task<Playlist> GetByIdWithDetailsAsync(int id);
    }
}
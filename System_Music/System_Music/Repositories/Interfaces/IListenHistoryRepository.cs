using System_Music.Models.SqlModels;
using System.Threading.Tasks;

namespace System_Music.Repositories.Interfaces
{
    public interface IListenHistoryRepository : IRepository<ListenHistory>
    {
        Task<List<ListenHistory>> GetByUserAsync(string userId);
        Task<List<ListenHistory>> GetByTrackAsync(int trackId);
        Task<List<ListenHistory>> GetByAlbumAsync(int albumId);
        Task<List<ListenHistory>> GetByArtistAsync(int artistId);
        Task<List<ListenHistory>> GetByPlaylistAsync(int playlistId);
        Task<bool> HasListenedAsync(string userId, EntityType entityType, int entityId);
    }
}
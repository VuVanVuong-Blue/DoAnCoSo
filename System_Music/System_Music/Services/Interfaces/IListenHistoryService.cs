using System_Music.Models.SqlModels;
using System.Threading.Tasks;

namespace System_Music.Services.Interfaces
{
    public interface IListenHistoryService
    {
        Task<List<ListenHistory>> GetAllListenHistoriesAsync();
        Task<List<ListenHistory>> GetListenHistoriesByUserAsync(string userId);
        Task<List<ListenHistory>> GetListenHistoriesByTrackAsync(int trackId);
        Task<List<ListenHistory>> GetListenHistoriesByAlbumAsync(int albumId);
        Task<List<ListenHistory>> GetListenHistoriesByArtistAsync(int artistId);
        Task<List<ListenHistory>> GetListenHistoriesByPlaylistAsync(int playlistId);
        Task AddListenHistoryAsync(ListenHistory listenHistory);
        Task<bool> HasListenedAsync(string userId, EntityType entityType, int entityId);
    }
}
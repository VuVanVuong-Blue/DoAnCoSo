using System_Music.Models.SqlModels;

namespace System_Music.Services.Interfaces
{
    public interface IAlbumService
    {
        Task<List<Album>> GetAllAlbumsAsync();
        Task<Album> GetAlbumByIdAsync(int id);
        Task<Album> GetAlbumByIdWithDetailsAsync(int id);
        Task AddAlbumAsync(Album album, int[] artistIds, int[] trackIds);
        Task UpdateAlbumAsync(Album album, int[] artistIds, int[] trackIds);
        Task DeleteAlbumAsync(int id);
        Task UpdateAlbumAsync(Album album);
        Task AddAlbumAsync(Album album);
        Task<List<Album>> SearchAlbumsAsync(string searchTerm);
        Task<List<Album>> GetAlbumsByArtistAsync(int artistId);
        Task<List<Album>> SyncAlbumFromZingMp3Async(string albumEncodeId);
    }
}
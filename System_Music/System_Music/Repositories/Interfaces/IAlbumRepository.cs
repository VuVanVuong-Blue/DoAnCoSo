using System_Music.Models.SqlModels;

namespace System_Music.Repositories.Interfaces
{
    public interface IAlbumRepository : IRepository<Album>
    {
        Task<List<Album>> GetAlbumsByArtistAsync(int artistId);
        Task<Album> GetByIdAsync(int id);
        Task<Album> GetByIdWithDetailsAsync(int id);
        Task<List<Album>> SyncAlbumFromZingMp3Async(string albumEncodeId);
    }
}
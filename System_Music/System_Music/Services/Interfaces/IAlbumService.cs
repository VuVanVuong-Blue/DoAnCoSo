using System_Music.Models.SqlModels;
using System_Music.Models.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace System_Music.Services.Interfaces
{
    public interface IAlbumService
    {
        Task<List<AlbumDto>> GetAllAlbumsAsync();
        Task<AlbumDto> GetAlbumByIdAsync(int id);
        Task<AlbumDto> GetAlbumByIdWithDetailsAsync(int id);
        Task<List<AlbumDto>> GetAlbumsByArtistAsync(int artistId);
        Task AddAlbumAsync(AlbumDto albumDto, int[] artistIds, int[] trackIds);
        Task UpdateAlbumAsync(AlbumDto albumDto, int[] artistIds, int[] trackIds);
        Task DeleteAlbumAsync(int id);
        Task<IEnumerable<AlbumDto>> SyncAlbumFromZingMp3Async(string albumEncodeId);
    }
}
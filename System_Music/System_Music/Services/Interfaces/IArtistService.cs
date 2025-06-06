using System_Music.Models.SqlModels;

namespace System_Music.Services.Interfaces
{
    public interface IArtistService
    {
        Task<List<Artist>> GetAllArtistsAsync();
        Task<Artist> GetArtistByIdAsync(int id);
        Task AddArtistAsync(Artist artist);
        Task UpdateArtistAsync(Artist artist);
        Task DeleteArtistAsync(int id);
        Task<List<Artist>> GetArtistsByCountryAsync(string country);
        Task<List<Artist>> SearchArtistsAsync(string searchTerm);
        Task<List<Track>> GetTracksByArtistIdAsync(int artistId);
        Task<List<Artist>> SyncArtistsFromZingMp3Async(string artistName = null, string artistId = null); // Thêm phương thức mới
    }
}
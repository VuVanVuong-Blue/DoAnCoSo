using System_Music.Models.SqlModels;
using System_Music.Models.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace System_Music.Services.Interfaces
{
    public interface IArtistService
    {
        Task<List<ArtistDto>> GetAllArtistsAsync();
        Task<ArtistDto> GetArtistByIdAsync(int id);
        Task<ArtistDto> GetArtistByIdWithDetailsAsync(int id);
        Task<List<ArtistDto>> GetArtistsByCountryAsync(string country);
        Task<List<TrackDto>> GetTracksByArtistIdAsync(int artistId);
        Task AddArtistAsync(ArtistDto artistDto);
        Task UpdateArtistAsync(ArtistDto artistDto);
        Task DeleteArtistAsync(int id);
        Task<IEnumerable<ArtistDto>> SyncArtistsFromZingMp3Async(string artistName, string artistId);
    }
}
using System.Collections.Generic;
using System.Threading.Tasks;
using System_Music.Models.SqlModels;

namespace System_Music.Repositories.Interfaces
{
    public interface ITrackArtistRepository
    {
        Task<ICollection<TrackArtist>> GetTrackArtistsByTrackIdAsync(int trackId);
        Task<List<TrackArtist>> GetAllAsync();
        Task<TrackArtist> GetByIdAsync(int trackId, int artistId);
        Task AddAsync(TrackArtist trackArtist);
        Task DeleteAsync(int trackId, int artistId);
        Task<List<TrackArtist>> GetArtistsByTrackAsync(int trackId);
        Task<List<TrackArtist>> GetTracksByArtistAsync(int artistId);
    }
}
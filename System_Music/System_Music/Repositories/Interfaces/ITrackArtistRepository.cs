using System.Collections.Generic;
using System.Threading.Tasks;
using System_Music.Models.SqlModels;

namespace System_Music.Services.Interfaces
{
    public interface ITrackArtistRepository
    {
        Task<ICollection<TrackArtist>> GetTrackArtistsByTrackIdAsync(int trackId);
    }
}
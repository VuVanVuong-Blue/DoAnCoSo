using System_Music.Models.SqlModels;
using System.Threading.Tasks;

namespace System_Music.Repositories.Interfaces
{
    public interface IPlaylistTrackRepository
    {
        Task<List<PlaylistTrack>> GetAllAsync();
        Task<PlaylistTrack> GetByIdAsync(int playlistId, int trackId);
        Task AddAsync(PlaylistTrack playlistTrack);
        Task DeleteAsync(int playlistId, int trackId);
        Task<List<PlaylistTrack>> GetTracksByPlaylistAsync(int playlistId);
    }
}
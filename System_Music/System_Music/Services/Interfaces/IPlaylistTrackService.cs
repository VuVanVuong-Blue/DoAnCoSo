using System_Music.Models.SqlModels;

namespace System_Music.Services.Interfaces
{
    public interface IPlaylistTrackService
    {
        Task<List<PlaylistTrack>> GetAllPlaylistTracksAsync();
        Task<PlaylistTrack> GetPlaylistTrackByIdAsync(int playlistId, int trackId);
        Task AddPlaylistTrackAsync(PlaylistTrack playlistTrack);
        Task DeletePlaylistTrackAsync(int playlistId, int trackId);
        Task<List<PlaylistTrack>> GetTracksByPlaylistAsync(int playlistId);
    }
}
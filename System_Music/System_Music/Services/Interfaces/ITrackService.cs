using System_Music.Models.SqlModels;

namespace System_Music.Services.Interfaces
{
    public interface ITrackService
    {
        Task<List<Track>> GetAllTracksAsync();
        Task<Track> GetTrackByIdAsync(int id); // Keep only one declaration of this method
        Task AddTrackAsync(Track track, int[] artistIds, int[] genreIds);
        Task UpdateTrackAsync(Track track, int[] artistIds, int[] genreIds);
        Task DeleteTrackAsync(int id);
        Task<List<Track>> GetTopTracksAsync(int count);
        Task<List<Track>> GetTracksByAlbumAsync(int albumId);
        Task<List<Track>> SearchTracksAsync(string searchTerm);
        Task<List<Track>> GetTracksByPlaylistAsync(int playlistId);
        Task<List<Track>> SyncTracksFromZingMp3Async(string encodeId);
        Task<List<Track>> SyncArtistSongsFromZingMp3Async(string artistId, int page = 1, int count = 5);
        Task<List<Track>> GetLikedTracksAsync(string userId);
        Task<Track> GetTrackByTitleAsync(string trackTitle);
        Task<Track> GetTrackByNormalizedTitleAsync(string normalizedTitle);
    }
}

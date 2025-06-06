using System_Music.Models.SqlModels;

namespace System_Music.Repositories.Interfaces
{
    public interface ITrackRepository : IRepository<Track>
    {
        Task<List<Track>> GetTopTracksAsync(int count);
        Task<List<Track>> GetTracksByAlbumAsync(int albumId);
        Task<List<Track>> SyncTracksFromZingMp3Async(string encodeId);
        Task<List<Track>> SyncArtistSongsFromZingMp3Async(string artistId, int page = 1, int count = 5);
        Task<Track> GetTrackByTitleAsync(string trackTitle); // Thêm phương thức mới
        Task<Track> GetTrackByNormalizedTitleAsync(string normalizedTitle); // Thêm mới
    }
}
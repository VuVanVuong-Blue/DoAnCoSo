using System_Music.Models.SqlModels;
using System_Music.Models.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace System_Music.Services.Interfaces
{
    public interface ITrackService
    {
        Task<List<TrackDto>> GetAllTracksAsync();
        Task<TrackDto> GetTrackByIdAsync(int id);
        Task<TrackDto> GetTrackWithDetailsAsync(int id);
        Task AddTrackAsync(TrackDto trackDto, int[] artistIds, int[] genreIds);
        Task UpdateTrackAsync(TrackDto trackDto, int[] artistIds, int[] genreIds);
        Task DeleteTrackAsync(int id);
        Task<List<TrackDto>> GetTopTracksAsync(int count);
        Task<List<TrackDto>> GetTracksByAlbumAsync(int albumId);
        Task<List<TrackDto>> SearchTracksAsync(string searchTerm);
        Task<List<TrackDto>> GetTracksByPlaylistAsync(int playlistId);
        Task<List<TrackDto>> SyncTracksFromZingMp3Async(string encodeId);
        Task<List<TrackDto>> SyncArtistSongsFromZingMp3Async(string artistId, int page = 1, int count = 5);
        Task<List<TrackDto>> GetLikedTracksAsync(string userId);
        Task<TrackDto> GetTrackByTitleAsync(string trackTitle);
        Task<TrackDto> GetTrackByNormalizedTitleAsync(string normalizedTitle);
        Task<List<TrackDto>> GetRecommendedTracksAsync(int trackId, int count);
    }
}

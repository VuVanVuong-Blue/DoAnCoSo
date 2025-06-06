using System_Music.Models.SqlModels;

namespace System_Music.Services.Interfaces
{
    public interface ILyricsTimingService
    {
        Task<List<LyricsTiming>> GetAllLyricsAsync();
        Task<LyricsTiming> GetLyricByIdAsync(int id);
        Task AddLyricAsync(LyricsTiming lyricsTiming);
        Task UpdateLyricAsync(LyricsTiming lyricsTiming);
        Task DeleteLyricAsync(int id);
        Task<List<LyricsTiming>> GetLyricsByTrackAsync(int trackId);
        Task<Track> GetTrackByTitleAsync(string trackTitle);
        Task<Track> GetTrackByNormalizedTitleAsync(string normalizedTitle);
    }
}
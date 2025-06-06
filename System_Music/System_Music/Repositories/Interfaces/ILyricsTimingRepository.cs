using System_Music.Models.SqlModels;

namespace System_Music.Repositories.Interfaces
{
    public interface ILyricsTimingRepository : IRepository<LyricsTiming>
    {
        Task<List<LyricsTiming>> GetLyricsByTrackAsync(int trackId);
    }
}
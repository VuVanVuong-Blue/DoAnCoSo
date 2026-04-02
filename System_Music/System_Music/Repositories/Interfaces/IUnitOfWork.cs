using System_Music.Repositories.Interfaces;

namespace System_Music.Repositories.Interfaces
{
    public interface IUnitOfWork : IDisposable
    {
        IUserRepository Users { get; }
        ITrackRepository Tracks { get; }
        IAlbumRepository Albums { get; }
        IArtistRepository Artists { get; }
        IGenreRepository Genres { get; }
        IPlaylistRepository Playlists { get; }
        IListenHistoryRepository ListenHistories { get; }
        ILikeTrackRepository LikeTracks { get; }
        IUserMediaRepository UserMedia { get; }
        IVideoRepository Videos { get; }
        IChartRankingRepository ChartRankings { get; }
        ITrackArtistRepository TrackArtists { get; }
        ITrackGenreRepository TrackGenres { get; }
        
        Task<int> CompleteAsync();
        Task BeginTransactionAsync();
        Task CommitTransactionAsync();
        Task RollbackTransactionAsync();
    }
}

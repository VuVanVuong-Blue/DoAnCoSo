using System_Music.Models.SqlModels;
using System_Music.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore.Storage;

namespace System_Music.Repositories.Implementations
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly SmartMusicDbContext _context;
        private IDbContextTransaction _transaction;

        public IUserRepository Users { get; }
        public ITrackRepository Tracks { get; }
        public IAlbumRepository Albums { get; }
        public IArtistRepository Artists { get; }
        public IGenreRepository Genres { get; }
        public IPlaylistRepository Playlists { get; }
        public IListenHistoryRepository ListenHistories { get; }
        public ILikeTrackRepository LikeTracks { get; }
        public IUserMediaRepository UserMedia { get; }
        public IVideoRepository Videos { get; }
        public IChartRankingRepository ChartRankings { get; }
        public ITrackArtistRepository TrackArtists { get; }
        public ITrackGenreRepository TrackGenres { get; }

        public UnitOfWork(
            SmartMusicDbContext context,
            IUserRepository userRepository,
            ITrackRepository trackRepository,
            IAlbumRepository albumRepository,
            IArtistRepository artistRepository,
            IGenreRepository genreRepository,
            IPlaylistRepository playlistRepository,
            IListenHistoryRepository listenHistoryRepository,
            ILikeTrackRepository likeTrackRepository,
            IUserMediaRepository userMediaRepository,
            IVideoRepository videoRepository,
            IChartRankingRepository chartRankingRepository,
            ITrackArtistRepository trackArtistRepository,
            ITrackGenreRepository trackGenreRepository)
        {
            _context = context;
            Users = userRepository;
            Tracks = trackRepository;
            Albums = albumRepository;
            Artists = artistRepository;
            Genres = genreRepository;
            Playlists = playlistRepository;
            ListenHistories = listenHistoryRepository;
            LikeTracks = likeTrackRepository;
            UserMedia = userMediaRepository;
            Videos = videoRepository;
            ChartRankings = chartRankingRepository;
            TrackArtists = trackArtistRepository;
            TrackGenres = trackGenreRepository;
        }

        public async Task<int> CompleteAsync()
        {
            return await _context.SaveChangesAsync();
        }

        public async Task BeginTransactionAsync()
        {
            _transaction = await _context.Database.BeginTransactionAsync();
        }

        public async Task CommitTransactionAsync()
        {
            if (_transaction != null)
            {
                await _transaction.CommitAsync();
                await _transaction.DisposeAsync();
                _transaction = null;
            }
        }

        public async Task RollbackTransactionAsync()
        {
            if (_transaction != null)
            {
                await _transaction.RollbackAsync();
                await _transaction.DisposeAsync();
                _transaction = null;
            }
        }

        public void Dispose()
        {
            _context.Dispose();
            _transaction?.Dispose();
        }
    }
}

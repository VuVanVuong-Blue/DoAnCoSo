using Microsoft.EntityFrameworkCore;
using System_Music.Models.SqlModels;
using System_Music.Repositories.Interfaces;

namespace System_Music.Repositories.Implementations
{
    public class ListenHistoryRepository : Repository<ListenHistory>, IListenHistoryRepository
    {
        private readonly SmartMusicDbContext _context;

        public ListenHistoryRepository(SmartMusicDbContext context) : base(context)
        {
            _context = context;
        }

        public override async Task<List<ListenHistory>> GetAllAsync()
        {
            var query = _context.ListenHistories.AsQueryable()
                .Include(lh => lh.User)
                .Include(lh => lh.Track)
                .Include(lh => lh.Album)
                .Include(lh => lh.Artist)
                .Include(lh => lh.Playlist);
            return await query.ToListAsync();
        }

        public async Task<List<ListenHistory>> GetByUserAsync(string userId)
        {
            var query = _context.ListenHistories.AsQueryable()
                .Where(lh => lh.UserId == userId)
                .Include(lh => lh.Track)
                .Include(lh => lh.Album)
                .Include(lh => lh.Artist)
                .Include(lh => lh.Playlist);
            return await query.ToListAsync();
        }

        public async Task<List<ListenHistory>> GetByTrackAsync(int trackId)
        {
            var query = _context.ListenHistories.AsQueryable()
                .Where(lh => lh.EntityType == EntityType.Track && lh.TrackId == trackId)
                .Include(lh => lh.User);
            return await query.ToListAsync();
        }

        public async Task<List<ListenHistory>> GetByAlbumAsync(int albumId)
        {
            var query = _context.ListenHistories.AsQueryable()
                .Where(lh => lh.EntityType == EntityType.Album && lh.AlbumId == albumId)
                .Include(lh => lh.User);
            return await query.ToListAsync();
        }

        public async Task<List<ListenHistory>> GetByArtistAsync(int artistId)
        {
            var query = _context.ListenHistories.AsQueryable()
                .Where(lh => lh.EntityType == EntityType.Artist && lh.ArtistId == artistId)
                .Include(lh => lh.User);
            return await query.ToListAsync();
        }

        public async Task<List<ListenHistory>> GetByPlaylistAsync(int playlistId)
        {
            var query = _context.ListenHistories.AsQueryable()
                .Where(lh => lh.EntityType == EntityType.Playlist && lh.PlaylistId == playlistId)
                .Include(lh => lh.User);
            return await query.ToListAsync();
        }

        public async Task<bool> HasListenedAsync(string userId, EntityType entityType, int entityId)
        {
            var query = _context.ListenHistories.AsQueryable()
                .Where(lh => lh.UserId == userId && lh.EntityType == entityType);
            switch (entityType)
            {
                case EntityType.Track:
                    return await query.AnyAsync(lh => lh.TrackId == entityId);
                case EntityType.Album:
                    return await query.AnyAsync(lh => lh.AlbumId == entityId);
                case EntityType.Artist:
                    return await query.AnyAsync(lh => lh.ArtistId == entityId);
                case EntityType.Playlist:
                    return await query.AnyAsync(lh => lh.PlaylistId == entityId);
                default:
                    return false;
            }
        }
    }
}
using Microsoft.EntityFrameworkCore;
using System_Music.Models.SqlModels;
using System_Music.Repositories.Interfaces;
using System_Music.Services.Interfaces;

namespace System_Music.Services.Implementations
{
    public class ListenHistoryService : IListenHistoryService
    {
        private readonly IListenHistoryRepository _listenHistoryRepository;
        private readonly SmartMusicDbContext _context;

        public ListenHistoryService(IListenHistoryRepository listenHistoryRepository, SmartMusicDbContext context)
        {
            _listenHistoryRepository = listenHistoryRepository;
            _context = context;
        }

        public async Task<List<ListenHistory>> GetAllListenHistoriesAsync()
        {
            return await _listenHistoryRepository.GetAllAsync();
        }

        public async Task<List<ListenHistory>> GetListenHistoriesByUserAsync(string userId)
        {
            return await _listenHistoryRepository.GetByUserAsync(userId);
        }

        public async Task<List<ListenHistory>> GetListenHistoriesByTrackAsync(int trackId)
        {
            return await _listenHistoryRepository.GetByTrackAsync(trackId);
        }

        public async Task<List<ListenHistory>> GetListenHistoriesByAlbumAsync(int albumId)
        {
            return await _listenHistoryRepository.GetByAlbumAsync(albumId);
        }

        public async Task<List<ListenHistory>> GetListenHistoriesByArtistAsync(int artistId)
        {
            return await _listenHistoryRepository.GetByArtistAsync(artistId);
        }

        public async Task<List<ListenHistory>> GetListenHistoriesByPlaylistAsync(int playlistId)
        {
            return await _listenHistoryRepository.GetByPlaylistAsync(playlistId);
        }

        public async Task AddListenHistoryAsync(ListenHistory listenHistory)
        {
            await _listenHistoryRepository.AddAsync(listenHistory);

            // Cập nhật PlayCount cho đối tượng tương ứng
            switch (listenHistory.EntityType)
            {
                case EntityType.Track:
                    var track = await _context.Tracks.FindAsync(listenHistory.TrackId);
                    if (track != null)
                    {
                        track.PlayCount = track.PlayCount + 1;
                        await _context.SaveChangesAsync();
                    }
                    break;
                case EntityType.Album:
                    var album = await _context.Albums.FindAsync(listenHistory.AlbumId);
                    if (album != null)
                    {
                        album.PlayCount = (album.PlayCount ?? 0) + 1;
                        await _context.SaveChangesAsync();
                    }
                    break;
                case EntityType.Artist:
                    var artist = await _context.Artists.FindAsync(listenHistory.ArtistId);
                    if (artist != null)
                    {
                        artist.PlayCount = (artist.PlayCount ?? 0) + 1;
                        await _context.SaveChangesAsync();
                    }
                    break;
                case EntityType.Playlist:
                    var playlist = await _context.Playlists.FindAsync(listenHistory.PlaylistId);
                    if (playlist != null)
                    {
                        playlist.PlayCount = (playlist.PlayCount ?? 0) + 1;
                        await _context.SaveChangesAsync();
                    }
                    break;
            }
        }

        public async Task<bool> HasListenedAsync(string userId, EntityType entityType, int entityId)
        {
            return await _listenHistoryRepository.HasListenedAsync(userId, entityType, entityId);
        }
    }
}
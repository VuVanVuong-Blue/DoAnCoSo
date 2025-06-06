using System_Music.Models.SqlModels;
using System_Music.Repositories.Interfaces;
using System_Music.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace System_Music.Services.Implementations
{
    public class PlaylistService : IPlaylistService
    {
        private readonly IPlaylistRepository _playlistRepository;
        private readonly SmartMusicDbContext _context;

        public PlaylistService(IPlaylistRepository playlistRepository, SmartMusicDbContext context)
        {
            _playlistRepository = playlistRepository;
            _context = context;
        }

        public async Task<List<Playlist>> GetAllPlaylistsAsync()
        {
            return await _context.Playlists
                .Include(p => p.ImageMedia)
                .ToListAsync();
        }

        public async Task<Playlist> GetPlaylistByIdAsync(int id)
        {
            return await _context.Playlists
                .Include(p => p.ImageMedia)
                .Include(p => p.PlaylistTracks)
                    .ThenInclude(pt => pt.Track)
                        .ThenInclude(t => t.Album) // Bao gồm thông tin Album
                .FirstOrDefaultAsync(p => p.PlaylistId == id);
        }


        public async Task AddPlaylistAsync(Playlist playlist)
        {
            await _playlistRepository.AddAsync(playlist);
        }

        public async Task UpdatePlaylistAsync(Playlist playlist)
        {
            await _playlistRepository.UpdateAsync(playlist);
        }

        public async Task DeletePlaylistAsync(int id)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var playlist = await _context.Playlists
                    .Include(p => p.PlaylistTracks)
                    .Include(p => p.ImageMedia)
                    .FirstOrDefaultAsync(p => p.PlaylistId == id);

                if (playlist == null)
                {
                    throw new Exception("Playlist không tồn tại!");
                }

                _context.PlaylistTracks.RemoveRange(playlist.PlaylistTracks);

                var userMedias = await _context.UserMedias
                    .Where(um => um.PlaylistId == id)
                    .ToListAsync();
                if (userMedias.Any())
                {
                    _context.UserMedias.RemoveRange(userMedias);
                }

                await _playlistRepository.DeleteAsync(id);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                throw new Exception($"Lỗi khi xóa playlist: {ex.Message}", ex);
            }
        }

        public async Task<List<Playlist>> GetUserPlaylistsAsync(string userId)
        {
            return await _context.Playlists
                .Include(p => p.ImageMedia)
                .Where(p => p.UserId == userId)
                .ToListAsync();
        }

        public Task CreatePlaylistAsync(Playlist playlist)
        {
            return _playlistRepository.AddAsync(playlist);
        }

        public async Task<List<Playlist>> GetPlaylistsByUserAsync(string userId)
        {
            return await _context.Playlists
                .Include(p => p.ImageMedia)
                .Where(p => p.UserId == userId)
                .ToListAsync();
        }
    }
}
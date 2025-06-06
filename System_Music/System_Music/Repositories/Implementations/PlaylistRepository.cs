using Microsoft.EntityFrameworkCore;
using System_Music.Models.SqlModels;
using System_Music.Repositories.Interfaces;

namespace System_Music.Repositories.Implementations
{
    public class PlaylistRepository : Repository<Playlist>, IPlaylistRepository
    {
        private readonly SmartMusicDbContext _context;

        public PlaylistRepository(SmartMusicDbContext context) : base(context)
        {
            _context = context;
        }

        public override async Task<List<Playlist>> GetAllAsync()
        {
            return await _context.Playlists
                .Include(p => p.User)
                .Include(p => p.ImageMedia) // Thêm để hỗ trợ RowButton
                .Include(p => p.PlaylistTracks)
                    .ThenInclude(pt => pt.Track)
                .ToListAsync();
        }

        public async Task<Playlist> GetByIdAsync(int id)
        {
            return await _context.Playlists
                .Include(p => p.User)
                .Include(p => p.ImageMedia) // Thêm để hỗ trợ RowButton
                .Include(p => p.PlaylistTracks)
                    .ThenInclude(pt => pt.Track)
                .FirstOrDefaultAsync(p => p.PlaylistId == id);
        }

        public async Task<Playlist> GetByIdWithDetailsAsync(int id)
        {
            return await _context.Playlists
                .Include(p => p.User)
                    .ThenInclude(u => u.AvatarMedia) // Thêm để hỗ trợ IndexPlayList
                .Include(p => p.ImageMedia) // Thêm để hỗ trợ RowButton
                .Include(p => p.PlaylistTracks)
                    .ThenInclude(pt => pt.Track)
                        .ThenInclude(t => t.Album)
                .Include(p => p.PlaylistTracks)
                    .ThenInclude(pt => pt.Track)
                        .ThenInclude(t => t.TrackArtists)
                            .ThenInclude(ta => ta.Artist)
                .Include(p => p.PlaylistTracks)
                    .ThenInclude(pt => pt.Track)
                        .ThenInclude(t => t.TrackGenres)
                            .ThenInclude(tg => tg.Genre)
                .FirstOrDefaultAsync(p => p.PlaylistId == id);
        }
    }
}
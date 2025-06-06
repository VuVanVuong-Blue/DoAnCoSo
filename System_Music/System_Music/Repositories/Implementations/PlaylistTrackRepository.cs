using Microsoft.EntityFrameworkCore;
using System_Music.Models.SqlModels;
using System_Music.Repositories.Interfaces;

namespace System_Music.Repositories.Implementations
{
    public class PlaylistTrackRepository : IPlaylistTrackRepository
    {
        private readonly SmartMusicDbContext _context;

        public PlaylistTrackRepository(SmartMusicDbContext context)
        {
            _context = context;
        }

        public async Task<List<PlaylistTrack>> GetAllAsync()
        {
            return await _context.PlaylistTracks
                .Include(pt => pt.Playlist)
                .Include(pt => pt.Track)
                .ToListAsync();
        }

        public async Task<PlaylistTrack> GetByIdAsync(int playlistId, int trackId)
        {
            return await _context.PlaylistTracks
                .Include(pt => pt.Playlist)
                .Include(pt => pt.Track)
                .FirstOrDefaultAsync(pt => pt.PlaylistId == playlistId && pt.TrackId == trackId);
        }

        public async Task AddAsync(PlaylistTrack playlistTrack)
        {
            await _context.PlaylistTracks.AddAsync(playlistTrack);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int playlistId, int trackId)
        {
            var playlistTrack = await _context.PlaylistTracks
                .FirstOrDefaultAsync(pt => pt.PlaylistId == playlistId && pt.TrackId == trackId);
            if (playlistTrack != null)
            {
                _context.PlaylistTracks.Remove(playlistTrack);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<List<PlaylistTrack>> GetTracksByPlaylistAsync(int playlistId)
        {
            return await _context.PlaylistTracks
                .Where(pt => pt.PlaylistId == playlistId)
                .Include(pt => pt.Track)
                    .ThenInclude(t => t.Album)
                .Include(pt => pt.Track)
                    .ThenInclude(t => t.TrackArtists)
                        .ThenInclude(ta => ta.Artist)
                .Include(pt => pt.Track)
                    .ThenInclude(t => t.TrackGenres)
                        .ThenInclude(tg => tg.Genre)
                .ToListAsync();
        }
    }
}
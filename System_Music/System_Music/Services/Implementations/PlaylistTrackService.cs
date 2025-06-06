using System_Music.Models.SqlModels;
using System_Music.Repositories.Interfaces;
using System_Music.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace System_Music.Services.Implementations
{
    public class PlaylistTrackService : IPlaylistTrackService
    {
        private readonly IPlaylistTrackRepository _playlistTrackRepository;
        private readonly SmartMusicDbContext _context;

        public PlaylistTrackService(IPlaylistTrackRepository playlistTrackRepository, SmartMusicDbContext context)
        {
            _playlistTrackRepository = playlistTrackRepository;
            _context = context;
        }

        public async Task<List<PlaylistTrack>> GetAllPlaylistTracksAsync()
        {
            return await _playlistTrackRepository.GetAllAsync();
        }

        public async Task<PlaylistTrack> GetPlaylistTrackByIdAsync(int playlistId, int trackId)
        {
            return await _playlistTrackRepository.GetByIdAsync(playlistId, trackId);
        }

        public async Task AddPlaylistTrackAsync(PlaylistTrack playlistTrack)
        {
            if (playlistTrack == null)
            {
                throw new ArgumentNullException(nameof(playlistTrack));
            }

            playlistTrack.AddedDate = DateTime.UtcNow;
            await _playlistTrackRepository.AddAsync(playlistTrack);
        }

        public async Task DeletePlaylistTrackAsync(int playlistId, int trackId)
        {
            var playlistTrack = await _playlistTrackRepository.GetByIdAsync(playlistId, trackId);
            if (playlistTrack == null)
            {
                throw new KeyNotFoundException($"PlaylistTrack with PlaylistId {playlistId} and TrackId {trackId} not found.");
            }

            await _playlistTrackRepository.DeleteAsync(playlistId, trackId);
        }

        public async Task<List<PlaylistTrack>> GetTracksByPlaylistAsync(int playlistId)
        {
            return await _context.PlaylistTracks
                .Where(pt => pt.PlaylistId == playlistId)
                .Include(pt => pt.Track)
                    .ThenInclude(t => t.TrackArtists)
                    .ThenInclude(ta => ta.Artist)
                .ToListAsync();
        }
    }
}
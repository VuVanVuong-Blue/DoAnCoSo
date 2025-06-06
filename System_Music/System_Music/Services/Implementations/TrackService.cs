using Microsoft.EntityFrameworkCore;
using System_Music.Models.SqlModels;
using System_Music.Repositories.Interfaces;
using System_Music.Services.Interfaces;
using System.Text;
using System.Globalization;

namespace System_Music.Services.Implementations
{
    public class TrackService : ITrackService
    {
        private readonly ITrackRepository _trackRepository;
        private readonly SmartMusicDbContext _context;
        private readonly ILikeTrackService _likeTrackService;

        public TrackService(ITrackRepository trackRepository, SmartMusicDbContext context, ILikeTrackService likeTrackService)
        {
            _trackRepository = trackRepository;
            _context = context;
            _likeTrackService = likeTrackService;
        }

        private string RemoveDiacritics(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return text;

            var normalizedString = text.Normalize(NormalizationForm.FormD);
            var stringBuilder = new StringBuilder();

            foreach (var c in normalizedString)
            {
                var unicodeCategory = CharUnicodeInfo.GetUnicodeCategory(c);
                if (unicodeCategory != UnicodeCategory.NonSpacingMark)
                {
                    stringBuilder.Append(c);
                }
            }

            return stringBuilder.ToString().Normalize(NormalizationForm.FormC).ToLower().Trim();
        }

        public async Task<Track> GetTrackByTitleAsync(string trackTitle)
        {
            if (string.IsNullOrWhiteSpace(trackTitle))
            {
                throw new ArgumentException("Track title không được để trống.");
            }

            var normalizedTitle = RemoveDiacritics(trackTitle);
            return await _trackRepository.GetTrackByTitleAsync(normalizedTitle);
        }

        public async Task<Track> GetTrackByNormalizedTitleAsync(string normalizedTitle)
        {
            if (string.IsNullOrWhiteSpace(normalizedTitle))
            {
                throw new ArgumentException("Normalized title không được để trống.");
            }

            return await _trackRepository.GetTrackByTitleAsync(normalizedTitle);
        }

        public async Task<List<Track>> GetAllTracksAsync()
        {
            return await _context.Tracks
                .Include(t => t.Album)
                .Include(t => t.TrackArtists)
                    .ThenInclude(ta => ta.Artist)
                .Include(t => t.TrackGenres)
                    .ThenInclude(tg => tg.Genre)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<Track> GetTrackByIdAsync(int id)
        {
            var track = await _context.Tracks
                .Include(t => t.Album)
                .Include(t => t.TrackArtists)
                    .ThenInclude(ta => ta.Artist)
                .Include(t => t.TrackGenres)
                    .ThenInclude(tg => tg.Genre)
                .AsNoTracking()
                .FirstOrDefaultAsync(t => t.TrackId == id);

            if (track == null)
            {
                Console.WriteLine($"Track with ID {id} not found in the database.");
            }
            else
            {
                Console.WriteLine($"Found track: {track.Title}, ID: {track.TrackId}, ImageUrl: {(track.ImageUrl ?? "null")}");
            }

            return track;
        }

        public async Task AddTrackAsync(Track track, int[] artistIds, int[] genreIds)
        {
            if (string.IsNullOrWhiteSpace(track.Title))
            {
                throw new ArgumentException("Tiêu đề bài hát không được để trống.");
            }

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                track.NormalizedTitle = RemoveDiacritics(track.Title);
                track.CreatedDate = DateTime.UtcNow;
                track.UpdatedDate = DateTime.UtcNow;
                await _trackRepository.AddAsync(track);
                await _context.SaveChangesAsync();

                if (artistIds != null && artistIds.Length > 0)
                {
                    foreach (var artistId in artistIds)
                    {
                        var artistExists = await _context.Artists.AnyAsync(a => a.ArtistId == artistId);
                        if (!artistExists)
                        {
                            Console.WriteLine($"Artist with ID {artistId} does not exist. Skipping...");
                            continue;
                        }

                        var trackArtist = new TrackArtist
                        {
                            TrackId = track.TrackId,
                            ArtistId = artistId,
                            Role = "Primary"
                        };
                        _context.TrackArtists.Add(trackArtist);
                    }
                }

                if (genreIds != null && genreIds.Length > 0)
                {
                    foreach (var genreId in genreIds)
                    {
                        var genreExists = await _context.Genres.AnyAsync(g => g.GenreId == genreId);
                        if (!genreExists)
                        {
                            Console.WriteLine($"Genre with ID {genreId} does not exist. Skipping...");
                            continue;
                        }

                        var trackGenre = new TrackGenre
                        {
                            TrackId = track.TrackId,
                            GenreId = genreId
                        };
                        _context.TrackGenres.Add(trackGenre);
                    }
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                throw new Exception($"Lỗi khi thêm bài hát: {ex.Message}", ex);
            }
        }

        public async Task UpdateTrackAsync(Track track, int[] artistIds, int[] genreIds)
        {
            if (string.IsNullOrWhiteSpace(track.Title))
            {
                throw new ArgumentException("Tiêu đề bài hát không được để trống.");
            }

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var existingTrack = await _trackRepository.GetByIdAsync(track.TrackId);
                if (existingTrack == null)
                {
                    throw new KeyNotFoundException($"Bài hát với ID {track.TrackId} không tồn tại.");
                }

                existingTrack.Title = track.Title;
                existingTrack.NormalizedTitle = RemoveDiacritics(track.Title);
                existingTrack.Duration = track.Duration;
                existingTrack.Description = track.Description;
                existingTrack.AudioUrl = track.AudioUrl;
                existingTrack.ImageUrl = track.ImageUrl;
                existingTrack.AlbumId = track.AlbumId;
                existingTrack.ZingMp3TrackId = track.ZingMp3TrackId;
                existingTrack.UpdatedDate = DateTime.UtcNow;

                await _trackRepository.UpdateAsync(existingTrack);

                var oldTrackArtists = _context.TrackArtists.Where(ta => ta.TrackId == track.TrackId);
                var oldTrackGenres = _context.TrackGenres.Where(tg => tg.TrackId == track.TrackId);
                _context.TrackArtists.RemoveRange(oldTrackArtists);
                _context.TrackGenres.RemoveRange(oldTrackGenres);

                if (artistIds != null && artistIds.Length > 0)
                {
                    foreach (var artistId in artistIds)
                    {
                        var artistExists = await _context.Artists.AnyAsync(a => a.ArtistId == artistId);
                        if (!artistExists)
                        {
                            Console.WriteLine($"Artist with ID {artistId} does not exist. Skipping...");
                            continue;
                        }

                        var trackArtist = new TrackArtist
                        {
                            TrackId = track.TrackId,
                            ArtistId = artistId,
                            Role = "Primary"
                        };
                        _context.TrackArtists.Add(trackArtist);
                    }
                }

                if (genreIds != null && genreIds.Length > 0)
                {
                    foreach (var genreId in genreIds)
                    {
                        var genreExists = await _context.Genres.AnyAsync(g => g.GenreId == genreId);
                        if (!genreExists)
                        {
                            Console.WriteLine($"Genre with ID {genreId} does not exist. Skipping...");
                            continue;
                        }

                        var trackGenre = new TrackGenre
                        {
                            TrackId = track.TrackId,
                            GenreId = genreId
                        };
                        _context.TrackGenres.Add(trackGenre);
                    }
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                throw new Exception($"Lỗi khi cập nhật bài hát: {ex.Message}", ex);
            }
        }

        public async Task DeleteTrackAsync(int id)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // Tìm bài hát cần xóa
                var track = await _trackRepository.GetByIdAsync(id);
                if (track == null)
                {
                    throw new KeyNotFoundException($"Track with ID {id} not found.");
                }

                // Xóa các bản ghi trong LyricsTimings liên quan đến Track
                var lyricsTimings = await _context.LyricsTimings
                    .Where(lt => lt.TrackId == id)
                    .ToListAsync();
                if (lyricsTimings.Any())
                {
                    _context.LyricsTimings.RemoveRange(lyricsTimings);
                    await _context.SaveChangesAsync();
                    Console.WriteLine($"[INFO] Removed {lyricsTimings.Count} LyricsTimings records for Track ID {id}");
                }

                // Xóa các bản ghi trong TrackArtists liên quan đến Track
                var trackArtists = await _context.TrackArtists
                    .Where(ta => ta.TrackId == id)
                    .ToListAsync();
                if (trackArtists.Any())
                {
                    _context.TrackArtists.RemoveRange(trackArtists);
                    await _context.SaveChangesAsync();
                    Console.WriteLine($"[INFO] Removed {trackArtists.Count} TrackArtists records for Track ID {id}");
                }

                // Xóa các bản ghi trong TrackGenres liên quan đến Track
                var trackGenres = await _context.TrackGenres
                    .Where(tg => tg.TrackId == id)
                    .ToListAsync();
                if (trackGenres.Any())
                {
                    _context.TrackGenres.RemoveRange(trackGenres);
                    await _context.SaveChangesAsync();
                    Console.WriteLine($"[INFO] Removed {trackGenres.Count} TrackGenres records for Track ID {id}");
                }

                // Xóa các bản ghi trong PlaylistTracks liên quan đến Track
                var playlistTracks = await _context.PlaylistTracks
                    .Where(pt => pt.TrackId == id)
                    .ToListAsync();
                if (playlistTracks.Any())
                {
                    _context.PlaylistTracks.RemoveRange(playlistTracks);
                    await _context.SaveChangesAsync();
                    Console.WriteLine($"[INFO] Removed {playlistTracks.Count} PlaylistTracks records for Track ID {id}");
                }

                // Xóa các bản ghi trong LikeTracks liên quan đến Track (nếu có)
                var likeTracks = await _context.LikeTracks
                    .Where(lt => lt.TrackId == id)
                    .ToListAsync();
                if (likeTracks.Any())
                {
                    _context.LikeTracks.RemoveRange(likeTracks);
                    await _context.SaveChangesAsync();
                    Console.WriteLine($"[INFO] Removed {likeTracks.Count} LikeTracks records for Track ID {id}");
                }

                // Sau khi xóa tất cả các quan hệ, xóa Track
                await _trackRepository.DeleteAsync(id);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
                Console.WriteLine($"[INFO] Track ID {id} deleted successfully");
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                throw new Exception($"Lỗi khi xóa bài hát: {ex.Message}", ex);
            }
        }

        public async Task<List<Track>> GetTopTracksAsync(int count)
        {
            return await _trackRepository.GetTopTracksAsync(count);
        }

        public async Task<List<Track>> GetTracksByAlbumAsync(int albumId)
        {
            return await _trackRepository.GetTracksByAlbumAsync(albumId);
        }

        public async Task<List<Track>> SearchTracksAsync(string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
            {
                return new List<Track>();
            }

            searchTerm = RemoveDiacritics(searchTerm) ?? searchTerm.ToLower().Trim();

            var tracksQuery = _context.Tracks
                .AsNoTracking()
                .AsSplitQuery()
                .Include(t => t.Album)
                .Include(t => t.TrackArtists)
                    .ThenInclude(ta => ta.Artist)
                .Include(t => t.TrackGenres)
                    .ThenInclude(tg => tg.Genre)
                .Where(t =>
                    (t.NormalizedTitle != null && t.NormalizedTitle.Contains(searchTerm)) ||
                    (t.Album != null && t.Album.NormalizedName != null && t.Album.NormalizedName.Contains(searchTerm)) ||
                    t.TrackArtists.Any(ta => ta.Artist != null && ta.Artist.NormalizedName != null && ta.Artist.NormalizedName.Contains(searchTerm))
                );

            var tracks = await tracksQuery.ToListAsync();

            var scoredTracks = tracks
                .Select(t => new
                {
                    Track = t,
                    Score = CalculateRelevanceScore(t, searchTerm)
                })
                .OrderByDescending(x => x.Score)
                .Take(5)
                .Select(x => x.Track)
                .ToList();

            return scoredTracks ?? new List<Track>();
        }

        private double CalculateRelevanceScore(Track track, string searchTerm)
        {
            double score = 0;

            if (track == null)
            {
                return score;
            }

            if (!string.IsNullOrEmpty(track.NormalizedTitle))
            {
                if (track.NormalizedTitle.Contains(searchTerm))
                {
                    score += 3.0;
                    if (track.NormalizedTitle.StartsWith(searchTerm))
                    {
                        score += 1.0;
                    }
                }
            }

            if (track.TrackArtists != null)
            {
                foreach (var ta in track.TrackArtists)
                {
                    if (ta?.Artist != null && !string.IsNullOrEmpty(ta.Artist.NormalizedName))
                    {
                        if (ta.Artist.NormalizedName.Contains(searchTerm))
                        {
                            score += 2.0;
                            if (ta.Artist.NormalizedName.StartsWith(searchTerm))
                            {
                                score += 0.5;
                            }
                        }
                    }
                }
            }

            if (track.Album != null && !string.IsNullOrEmpty(track.Album.NormalizedName))
            {
                if (track.Album.NormalizedName.Contains(searchTerm))
                {
                    score += 1.0;
                }
            }

            if (track.PlayCount > 0)
            {
                score += Math.Min(track.PlayCount / 1000.0, 1.0);
            }
            if (track.LikeCount > 0)
            {
                score += Math.Min(track.LikeCount / 500.0, 0.5);
            }

            return score;
        }

        public async Task<List<Track>> GetTracksByPlaylistAsync(int playlistId)
        {
            return await _context.PlaylistTracks
                .Where(pt => pt.PlaylistId == playlistId)
                .Include(pt => pt.Track)
                    .ThenInclude(t => t.TrackArtists)
                    .ThenInclude(ta => ta.Artist)
                .Include(pt => pt.Track)
                    .ThenInclude(t => t.TrackGenres)
                    .ThenInclude(tg => tg.Genre)
                .Select(pt => pt.Track)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<List<Track>> SyncTracksFromZingMp3Async(string encodeId)
        {
            if (string.IsNullOrWhiteSpace(encodeId))
            {
                throw new ArgumentException("EncodeId không được để trống.");
            }

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var tracks = await _trackRepository.SyncTracksFromZingMp3Async(encodeId);
                if (!tracks.Any())
                {
                    throw new Exception("Không tìm thấy bài hát với encodeId đã cung cấp hoặc bài hát đã tồn tại.");
                }
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
                return tracks;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                throw new Exception($"Lỗi khi đồng bộ bài hát từ Zing MP3: {ex.Message}", ex);
            }
        }

        public async Task<List<Track>> SyncArtistSongsFromZingMp3Async(string artistId, int page = 1, int count = 5)
        {
            if (string.IsNullOrWhiteSpace(artistId))
            {
                throw new ArgumentException("ArtistId không được để trống.");
            }

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var tracks = await _trackRepository.SyncArtistSongsFromZingMp3Async(artistId, page, count);
                if (!tracks.Any())
                {
                    throw new Exception("Không tìm thấy bài hát nào cho nghệ sĩ với artistId đã cung cấp.");
                }
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
                return tracks;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                throw new Exception($"Lỗi khi đồng bộ bài hát của nghệ sĩ từ Zing MP3: {ex.Message}", ex);
            }
        }

        public async Task<List<Track>> GetLikedTracksAsync(string userId)
        {
            if (string.IsNullOrWhiteSpace(userId))
            {
                throw new ArgumentException("UserId không được để trống.");
            }

            var likeTracks = await _likeTrackService.GetLikesByUserAsync(userId);
            var likedTracks = likeTracks.Select(lt => lt.Track).ToList();
            var likeDates = likeTracks.ToDictionary(lt => lt.TrackId, lt => lt.LikeDate == default ? DateTime.UtcNow : lt.LikeDate);

            foreach (var track in likedTracks)
            {
                await _context.Entry(track)
                    .Collection(t => t.TrackArtists)
                    .Query()
                    .Include(ta => ta.Artist)
                    .LoadAsync();

                await _context.Entry(track)
                    .Reference(t => t.Album)
                    .LoadAsync();
            }
            return likedTracks.OrderByDescending(t => likeDates.TryGetValue(t.TrackId, out var date) ? date : DateTime.UtcNow).ToList();
        }
    }
}
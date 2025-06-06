using Microsoft.EntityFrameworkCore;
using System_Music.Models.SqlModels;
using System_Music.Repositories.Interfaces;
using System_Music.Services.Interfaces;
using System.Text;
using System.Globalization;

namespace System_Music.Services.Implementations
{
    public class AlbumService : IAlbumService
    {
        private readonly IAlbumRepository _albumRepository;
        private readonly SmartMusicDbContext _context;

        public AlbumService(IAlbumRepository albumRepository, SmartMusicDbContext context)
        {
            _albumRepository = albumRepository;
            _context = context;
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

        public async Task<List<Album>> GetAllAlbumsAsync()
        {
            return await _albumRepository.GetAllAsync();
        }

        public async Task<Album> GetAlbumByIdAsync(int id)
        {
            return await _albumRepository.GetByIdAsync(id);
        }

        public async Task<Album> GetAlbumByIdWithDetailsAsync(int id)
        {
            return await _context.Albums
                .Include(a => a.Tracks)
                    .ThenInclude(t => t.TrackArtists)
                        .ThenInclude(ta => ta.Artist)
                .Include(a => a.AlbumArtists)
                    .ThenInclude(aa => aa.Artist)
                .FirstOrDefaultAsync(a => a.AlbumId == id);
        }

        public async Task AddAlbumAsync(Album album, int[] artistIds, int[] trackIds)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                if (string.IsNullOrWhiteSpace(album.Name))
                {
                    throw new ArgumentException("Tên album không được để trống.");
                }

                album.NormalizedName = RemoveDiacritics(album.Name);
                album.CreatedDate = DateTime.UtcNow;
                album.UpdatedDate = DateTime.UtcNow;

                await _albumRepository.AddAsync(album);

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

                        var albumArtist = new AlbumArtist
                        {
                            AlbumId = album.AlbumId,
                            ArtistId = artistId
                        };
                        _context.AlbumArtists.Add(albumArtist);
                    }
                }

                if (trackIds != null && trackIds.Length > 0)
                {
                    foreach (var trackId in trackIds)
                    {
                        var track = await _context.Tracks.FindAsync(trackId);
                        if (track == null)
                        {
                            Console.WriteLine($"Track with ID {trackId} does not exist. Skipping...");
                            continue;
                        }

                        track.AlbumId = album.AlbumId;
                        _context.Tracks.Update(track);
                    }
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                Console.WriteLine($"Error adding album: {ex.Message}");
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"Inner exception: {ex.InnerException.Message}");
                }
                throw;
            }
        }

        public async Task UpdateAlbumAsync(Album album, int[] artistIds, int[] trackIds)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var existingAlbum = await _albumRepository.GetByIdAsync(album.AlbumId);
                if (existingAlbum == null)
                {
                    throw new KeyNotFoundException($"Album with ID {album.AlbumId} not found.");
                }

                if (string.IsNullOrWhiteSpace(album.Name))
                {
                    throw new ArgumentException("Tên album không được để trống.");
                }

                existingAlbum.Name = album.Name;
                existingAlbum.NormalizedName = RemoveDiacritics(album.Name);
                existingAlbum.Image = album.Image;
                existingAlbum.ReleaseDate = album.ReleaseDate;
                existingAlbum.UpdatedDate = DateTime.UtcNow;
                existingAlbum.CreatedDate = album.CreatedDate;

                var oldAlbumArtists = _context.AlbumArtists.Where(aa => aa.AlbumId == album.AlbumId);
                _context.AlbumArtists.RemoveRange(oldAlbumArtists);

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

                        var albumArtist = new AlbumArtist
                        {
                            AlbumId = album.AlbumId,
                            ArtistId = artistId
                        };
                        _context.AlbumArtists.Add(albumArtist);
                    }
                }

                var existingTracks = await _context.Tracks.Where(t => t.AlbumId == album.AlbumId).ToListAsync();
                foreach (var track in existingTracks)
                {
                    if (trackIds == null || !trackIds.Contains(track.TrackId))
                    {
                        track.AlbumId = null;
                        _context.Tracks.Update(track);
                    }
                }

                if (trackIds != null && trackIds.Length > 0)
                {
                    foreach (var trackId in trackIds)
                    {
                        var track = await _context.Tracks.FindAsync(trackId);
                        if (track == null)
                        {
                            Console.WriteLine($"Track with ID {trackId} does not exist. Skipping...");
                            continue;
                        }

                        track.AlbumId = album.AlbumId;
                        _context.Tracks.Update(track);
                    }
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                Console.WriteLine($"Error updating album: {ex.Message}");
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"Inner exception: {ex.InnerException.Message}");
                }
                throw;
            }
        }

        public async Task DeleteAlbumAsync(int id)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var album = await _context.Albums
                    .Include(a => a.AlbumArtists)
                    .Include(a => a.Tracks)
                    .FirstOrDefaultAsync(a => a.AlbumId == id);

                if (album == null)
                {
                    throw new KeyNotFoundException($"Album with ID {id} not found.");
                }

                Console.WriteLine($"Removing {album.AlbumArtists.Count} AlbumArtists...");
                _context.AlbumArtists.RemoveRange(album.AlbumArtists);

                Console.WriteLine($"Updating {album.Tracks.Count} Tracks to set AlbumId to NULL...");
                foreach (var track in album.Tracks)
                {
                    track.AlbumId = null;
                    _context.Tracks.Update(track);
                }

                var userMedias = await _context.UserMedias
                    .Where(um => um.PlaylistId == id)
                    .ToListAsync();
                if (userMedias.Any())
                {
                    Console.WriteLine($"Removing {userMedias.Count} UserMedias...");
                    _context.UserMedias.RemoveRange(userMedias);
                }

                Console.WriteLine("ChangeTracker entries before SaveChanges:");
                foreach (var entry in _context.ChangeTracker.Entries())
                {
                    Console.WriteLine($"Entity: {entry.Entity}, State: {entry.State}");
                }

                Console.WriteLine($"Deleting Album with AlbumId={id}...");
                await _albumRepository.DeleteAsync(id);

                await _context.SaveChangesAsync();
                Console.WriteLine($"Album with AlbumId={id} deleted successfully.");
                await transaction.CommitAsync();
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                Console.WriteLine($"Error deleting album: {ex.Message}");
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"Inner exception: {ex.InnerException.Message}");
                }
                throw;
            }
        }

        public async Task UpdateAlbumAsync(Album album)
        {
            if (string.IsNullOrWhiteSpace(album.Name))
            {
                throw new ArgumentException("Tên album không được để trống.");
            }

            var existingAlbum = await _albumRepository.GetByIdAsync(album.AlbumId);
            if (existingAlbum == null)
            {
                throw new KeyNotFoundException($"Album with ID {album.AlbumId} not found.");
            }

            existingAlbum.Name = album.Name;
            existingAlbum.NormalizedName = RemoveDiacritics(album.Name);
            existingAlbum.Image = album.Image;
            existingAlbum.ReleaseDate = album.ReleaseDate;
            existingAlbum.UpdatedDate = DateTime.UtcNow;

            await _albumRepository.UpdateAsync(existingAlbum);
        }

        public async Task AddAlbumAsync(Album album)
        {
            if (string.IsNullOrWhiteSpace(album.Name))
            {
                throw new ArgumentException("Tên album không được để trống.");
            }

            album.NormalizedName = RemoveDiacritics(album.Name);
            album.CreatedDate = DateTime.UtcNow;
            album.UpdatedDate = DateTime.UtcNow;

            await _albumRepository.AddAsync(album);
        }

        public async Task<List<Album>> SearchAlbumsAsync(string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
            {
                return new List<Album>();
            }

            searchTerm = RemoveDiacritics(searchTerm) ?? searchTerm.ToLower().Trim();

            var albumsQuery = _context.Albums
                .AsNoTracking()
                .AsSplitQuery()
                .Include(a => a.AlbumArtists)
                    .ThenInclude(aa => aa.Artist)
                .Where(a =>
                    (a.NormalizedName != null && a.NormalizedName.Contains(searchTerm)) ||
                    a.AlbumArtists.Any(aa => aa.Artist != null && aa.Artist.NormalizedName != null && aa.Artist.NormalizedName.Contains(searchTerm))
                );

            var albums = await albumsQuery.ToListAsync();

            var scoredAlbums = albums
                .Select(a => new
                {
                    Album = a,
                    Score = CalculateRelevanceScore(a, searchTerm)
                })
                .OrderByDescending(x => x.Score)
                .Take(5)
                .Select(x => x.Album)
                .ToList();

            return scoredAlbums ?? new List<Album>();
        }

        private double CalculateRelevanceScore(Album album, string searchTerm)
        {
            double score = 0;

            if (album == null)
            {
                return score;
            }

            if (!string.IsNullOrEmpty(album.NormalizedName))
            {
                if (album.NormalizedName.Contains(searchTerm))
                {
                    score += 3.0;
                    if (album.NormalizedName.StartsWith(searchTerm))
                    {
                        score += 1.0;
                    }
                }
            }

            if (album.AlbumArtists != null)
            {
                foreach (var aa in album.AlbumArtists)
                {
                    if (aa?.Artist != null && !string.IsNullOrEmpty(aa.Artist.NormalizedName))
                    {
                        if (aa.Artist.NormalizedName.Contains(searchTerm))
                        {
                            score += 2.0;
                            if (aa.Artist.NormalizedName.StartsWith(searchTerm))
                            {
                                score += 0.5;
                            }
                        }
                    }
                }
            }

            return score;
        }

        public async Task<List<Album>> GetAlbumsByArtistAsync(int artistId)
        {
            return await _context.AlbumArtists
                .Where(aa => aa.ArtistId == artistId)
                .Include(aa => aa.Album)
                .Select(aa => aa.Album)
                .ToListAsync();
        }

        public async Task<List<Album>> SyncAlbumFromZingMp3Async(string albumEncodeId)
        {
            if (string.IsNullOrWhiteSpace(albumEncodeId))
            {
                throw new ArgumentException("Album EncodeId không được để trống.");
            }

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var albums = await _albumRepository.SyncAlbumFromZingMp3Async(albumEncodeId);
                if (!albums.Any())
                {
                    throw new Exception("Không tìm thấy album với encodeId đã cung cấp hoặc album đã tồn tại.");
                }
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
                return albums;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                throw new Exception($"Lỗi khi đồng bộ album từ Zing MP3: {ex.Message}", ex);
            }
        }
    }
}
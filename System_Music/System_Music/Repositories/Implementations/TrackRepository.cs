using System.Globalization;
using System.Net.Http;
using System.Text.Json;
using System.Text;
using Microsoft.EntityFrameworkCore;
using System_Music.Models.SqlModels;
using System_Music.Repositories.Interfaces;
using Microsoft.Extensions.Configuration;

namespace System_Music.Repositories.Implementations
{
    public class TrackRepository : Repository<Track>, ITrackRepository
    {
        private readonly SmartMusicDbContext _context;
        private readonly HttpClient _httpClient;
        private readonly string _zingMp3ApiUrl;

        public TrackRepository(SmartMusicDbContext context, HttpClient httpClient, IConfiguration configuration) : base(context)
        {
            _context = context;
            _httpClient = httpClient;
            _zingMp3ApiUrl = configuration["ZingMp3ApiUrl"] ?? "http://localhost:5000";
        }
        public async Task<Track> GetTrackByNormalizedTitleAsync(string normalizedTitle)
        {
            if (string.IsNullOrWhiteSpace(normalizedTitle))
            {
                return null;
            }

            return await _context.Tracks
                .Include(t => t.Album)
                .Include(t => t.TrackArtists)
                    .ThenInclude(ta => ta.Artist)
                .Include(t => t.TrackGenres)
                    .ThenInclude(tg => tg.Genre)
                .FirstOrDefaultAsync(t => t.NormalizedTitle == normalizedTitle);
        }
        public async Task<List<Track>> GetTopTracksAsync(int count)
        {
            return await _context.Tracks
                .OrderByDescending(t => t.PlayCount)
                .Take(count)
                .Include(t => t.Album)
                .Include(t => t.TrackArtists)
                    .ThenInclude(ta => ta.Artist)
                .Include(t => t.TrackGenres)
                    .ThenInclude(tg => tg.Genre)
                .ToListAsync();
        }

        public async Task<List<Track>> GetTracksByAlbumAsync(int albumId)
        {
            return await _context.Tracks
                .Where(t => t.AlbumId == albumId)
                .Include(t => t.Album)
                .Include(t => t.TrackArtists)
                    .ThenInclude(ta => ta.Artist)
                .Include(t => t.TrackGenres)
                    .ThenInclude(tg => tg.Genre)
                .ToListAsync();
        }

        public override async Task<List<Track>> GetAllAsync()
        {
            return await _context.Tracks
                .Include(t => t.Album)
                .Include(t => t.TrackArtists)
                    .ThenInclude(ta => ta.Artist)
                .Include(t => t.TrackGenres)
                    .ThenInclude(tg => tg.Genre)
                .ToListAsync();
        }

        public async Task<Track> GetByIdAsync(int id)
        {
            return await _context.Tracks
                .AsTracking()
                .Include(t => t.Album)
                .Include(t => t.TrackArtists)
                    .ThenInclude(ta => ta.Artist)
                .Include(t => t.TrackGenres)
                    .ThenInclude(tg => tg.Genre)
                .FirstOrDefaultAsync(t => t.TrackId == id);
        }

        public async Task DeleteAsync(int id)
        {
            var track = await _context.Tracks
                .Include(t => t.TrackArtists)
                .Include(t => t.TrackGenres)
                .Include(t => t.PlaylistTracks)
                .Include(t => t.ListenHistories)
                .Include(t => t.LikeTracks)
                .FirstOrDefaultAsync(t => t.TrackId == id);

            if (track != null)
            {
                _context.TrackArtists.RemoveRange(track.TrackArtists);
                _context.TrackGenres.RemoveRange(track.TrackGenres);
                _context.PlaylistTracks.RemoveRange(track.PlaylistTracks);
                _context.ListenHistories.RemoveRange(track.ListenHistories);
                _context.LikeTracks.RemoveRange(track.LikeTracks);
                _context.Tracks.Remove(track);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<Track> GetTrackByTitleAsync(string trackTitle)
        {
            if (string.IsNullOrWhiteSpace(trackTitle))
            {
                return null;
            }

            var normalizedTitle = RemoveDiacritics(trackTitle);
            return await _context.Tracks
                .Include(t => t.Album)
                .Include(t => t.TrackArtists)
                    .ThenInclude(ta => ta.Artist)
                .Include(t => t.TrackGenres)
                    .ThenInclude(tg => tg.Genre)
                .FirstOrDefaultAsync(t => t.NormalizedTitle == normalizedTitle);
        }

        public async Task<List<Track>> SyncTracksFromZingMp3Async(string encodeId)
        {
            // Code hiện tại không đổi
            try
            {
                if (string.IsNullOrWhiteSpace(encodeId))
                {
                    Console.WriteLine("[DEBUG] EncodeId is empty or null.");
                    return new List<Track>();
                }

                var songUrl = $"{_zingMp3ApiUrl}/api/song/{encodeId}";
                var songInfoUrl = $"{_zingMp3ApiUrl}/api/song-info/{encodeId}";
                Console.WriteLine($"[DEBUG] Calling Node.js API - Song: {songUrl}");
                Console.WriteLine($"[DEBUG] Calling Node.js API - Song Info: {songInfoUrl}");

                var songResponse = await _httpClient.GetAsync(songUrl);
                if (!songResponse.IsSuccessStatusCode)
                {
                    Console.WriteLine($"[DEBUG] API call to /api/song failed: Status={songResponse.StatusCode}");
                    return new List<Track>();
                }

                var songJson = await songResponse.Content.ReadAsStringAsync();
                Console.WriteLine($"[DEBUG] Raw Response from /api/song: {songJson}");
                var songDoc = JsonDocument.Parse(songJson);
                if (songDoc.RootElement.GetProperty("err").GetInt32() != 0)
                {
                    Console.WriteLine($"[DEBUG] API error for /api/song: {songDoc.RootElement.GetProperty("msg").GetString()}");
                    return new List<Track>();
                }

                var songInfoResponse = await _httpClient.GetAsync(songInfoUrl);
                if (!songInfoResponse.IsSuccessStatusCode)
                {
                    Console.WriteLine($"[DEBUG] API call to /api/song-info failed: Status={songInfoResponse.StatusCode}");
                    return new List<Track>();
                }

                var songInfoJson = await songInfoResponse.Content.ReadAsStringAsync();
                Console.WriteLine($"[DEBUG] Raw Response from /api/song-info: {songInfoJson}");
                var songInfoDoc = JsonDocument.Parse(songInfoJson);
                if (songInfoDoc.RootElement.GetProperty("err").GetInt32() != 0)
                {
                    Console.WriteLine($"[DEBUG] API error for /api/song-info: {songInfoDoc.RootElement.GetProperty("msg").GetString()}");
                    return new List<Track>();
                }

                var tracks = new List<Track>();
                var songData = songDoc.RootElement.GetProperty("data");
                var songInfoData = songInfoDoc.RootElement.GetProperty("data");

                var existingTrack = await _context.Tracks
                    .FirstOrDefaultAsync(t => t.ZingMp3TrackId == encodeId);

                if (existingTrack != null)
                {
                    Console.WriteLine($"[DEBUG] Track with encodeId {encodeId} already exists in database.");
                    tracks.Add(existingTrack);
                    return tracks;
                }

                var artistIds = new List<int>();
                if (songInfoData.TryGetProperty("artists", out var artistsElement) && artistsElement.ValueKind == JsonValueKind.Array)
                {
                    foreach (var artist in artistsElement.EnumerateArray())
                    {
                        if (artist.TryGetProperty("id", out var artistIdElement) && artist.TryGetProperty("name", out var artistNameElement))
                        {
                            var artistZingId = artistIdElement.GetString();
                            var artistName = artistNameElement.GetString();
                            var artistImage = artist.TryGetProperty("thumbnail", out var thumbnail) ? thumbnail.GetString() : null;

                            var existingArtist = await _context.Artists
                                .FirstOrDefaultAsync(a => a.ZingMp3ArtistId == artistZingId);

                            if (existingArtist == null)
                            {
                                existingArtist = new Artist
                                {
                                    Name = artistName,
                                    NormalizedName = RemoveDiacritics(artistName),
                                    Image = artistImage,
                                    ZingMp3ArtistId = artistZingId,
                                    CreatedDate = DateTime.UtcNow,
                                    IsActive = true
                                };
                                await _context.Artists.AddAsync(existingArtist);
                                await _context.SaveChangesAsync();
                            }
                            artistIds.Add(existingArtist.ArtistId);
                        }
                    }
                }

                int? albumId = null;
                if (songInfoData.TryGetProperty("album", out var albumElement) && albumElement.ValueKind != JsonValueKind.Null)
                {
                    if (albumElement.TryGetProperty("encodeId", out var albumZingIdElement) &&
                        albumElement.TryGetProperty("title", out var albumTitleElement))
                    {
                        var albumZingId = albumZingIdElement.GetString();
                        var albumTitle = albumTitleElement.GetString();
                        var albumImage = albumElement.TryGetProperty("thumbnail", out var thumbnail) ? thumbnail.GetString() : null;

                        DateTime releaseDate = DateTime.UtcNow;
                        if (songInfoData.TryGetProperty("releaseDate", out var releaseDateElement))
                        {
                            if (releaseDateElement.ValueKind == JsonValueKind.Number)
                            {
                                long timestamp = releaseDateElement.GetInt64();
                                DateTime epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
                                releaseDate = epoch.AddSeconds(timestamp).ToUniversalTime();
                                Console.WriteLine($"[DEBUG] Converted timestamp {timestamp} to releaseDate: {releaseDate}");
                            }
                            else if (releaseDateElement.ValueKind == JsonValueKind.String)
                            {
                                var releaseDateStr = releaseDateElement.GetString();
                                if (!string.IsNullOrEmpty(releaseDateStr) && !DateTime.TryParse(releaseDateStr, out releaseDate))
                                {
                                    Console.WriteLine($"[DEBUG] Invalid releaseDate format: {releaseDateStr}. Using default: {releaseDate}");
                                }
                            }
                        }

                        var existingAlbum = await _context.Albums
                            .FirstOrDefaultAsync(a => a.ZingMp3AlbumId == albumZingId);

                        if (existingAlbum == null)
                        {
                            existingAlbum = new Album
                            {
                                Name = albumTitle,
                                NormalizedName = RemoveDiacritics(albumTitle),
                                Image = albumImage,
                                ZingMp3AlbumId = albumZingId,
                                ReleaseDate = releaseDate,
                                CreatedDate = DateTime.UtcNow
                            };
                            await _context.Albums.AddAsync(existingAlbum);
                            await _context.SaveChangesAsync();

                            foreach (var artistId in artistIds)
                            {
                                var albumArtist = new AlbumArtist
                                {
                                    AlbumId = existingAlbum.AlbumId,
                                    ArtistId = artistId
                                };
                                _context.AlbumArtists.Add(albumArtist);
                            }
                            await _context.SaveChangesAsync();
                        }
                        albumId = existingAlbum.AlbumId;
                    }
                }

                var genreIds = new List<int>();
                if (songInfoData.TryGetProperty("genres", out var genresElement) && genresElement.ValueKind == JsonValueKind.Array)
                {
                    foreach (var genre in genresElement.EnumerateArray())
                    {
                        if (genre.TryGetProperty("id", out var genreIdElement) && genre.TryGetProperty("name", out var genreNameElement))
                        {
                            var genreZingId = genreIdElement.GetString();
                            var genreName = genreNameElement.GetString();

                            var existingGenre = await _context.Genres
                                .FirstOrDefaultAsync(g => g.ZingMp3GenreId == genreZingId);

                            if (existingGenre == null)
                            {
                                existingGenre = new Genre
                                {
                                    Name = genreName,
                                    ZingMp3GenreId = genreZingId,
                                    Description = $"Thể loại từ Zing MP3 (ID: {genreZingId})"
                                };
                                await _context.Genres.AddAsync(existingGenre);
                                await _context.SaveChangesAsync();
                            }
                            genreIds.Add(existingGenre.GenreId);
                        }
                    }
                }

                string audioUrl = null;
                if (songData.TryGetProperty("streaming", out var streamingElement))
                {
                    if (streamingElement.TryGetProperty("mp3_128", out var mp3_128Element))
                    {
                        audioUrl = mp3_128Element.GetString();
                        if (!string.IsNullOrEmpty(audioUrl) && !audioUrl.StartsWith("http"))
                            audioUrl = $"https:{audioUrl}";
                    }
                    else if (streamingElement.TryGetProperty("mp3_320", out var mp3_320Element))
                    {
                        audioUrl = mp3_320Element.GetString();
                        if (!string.IsNullOrEmpty(audioUrl) && !audioUrl.StartsWith("http"))
                            audioUrl = $"https:{audioUrl}";
                    }
                }

                if (string.IsNullOrEmpty(audioUrl))
                {
                    Console.WriteLine("[DEBUG] No streaming URL found for the track.");
                    return new List<Track>();
                }

                var track = new Track
                {
                    Title = songInfoData.GetProperty("title").GetString(),
                    NormalizedTitle = RemoveDiacritics(songInfoData.GetProperty("title").GetString()),
                    Duration = songInfoData.GetProperty("duration").GetInt32(),
                    Description = $"Bài hát từ Zing MP3 (ID: {encodeId})",
                    AudioUrl = audioUrl,
                    ImageUrl = songInfoData.GetProperty("thumbnailM").GetString(),
                    ZingMp3TrackId = encodeId,
                    AlbumId = albumId,
                    CreatedDate = DateTime.UtcNow,
                    UpdatedDate = DateTime.UtcNow,
                    PlayCount = 0,
                    LikeCount = 0
                };

                await _context.Tracks.AddAsync(track);
                await _context.SaveChangesAsync();

                foreach (var artistId in artistIds)
                {
                    var trackArtist = new TrackArtist
                    {
                        TrackId = track.TrackId,
                        ArtistId = artistId,
                        Role = "Primary"
                    };
                    _context.TrackArtists.Add(trackArtist);
                }

                foreach (var genreId in genreIds)
                {
                    var trackGenre = new TrackGenre
                    {
                        TrackId = track.TrackId,
                        GenreId = genreId
                    };
                    _context.TrackGenres.Add(trackGenre);
                }

                if (songInfoData.TryGetProperty("lyrics", out var lyricsElement) && lyricsElement.ValueKind == JsonValueKind.Array)
                {
                    foreach (var lyricItem in lyricsElement.EnumerateArray())
                    {
                        if (lyricItem.TryGetProperty("words", out var wordsElement) && wordsElement.ValueKind == JsonValueKind.Array)
                        {
                            foreach (var word in wordsElement.EnumerateArray())
                            {
                                if (word.TryGetProperty("startTime", out var startTimeElement) &&
                                    word.TryGetProperty("endTime", out var endTimeElement) &&
                                    word.TryGetProperty("data", out var dataElement))
                                {
                                    var lyricTiming = new LyricsTiming
                                    {
                                        TrackId = track.TrackId,
                                        StartTime = startTimeElement.GetInt32(),
                                        EndTime = endTimeElement.GetInt32(),
                                        LyricText = dataElement.GetString(),
                                        CreatedDate = DateTime.UtcNow
                                    };
                                    _context.LyricsTimings.Add(lyricTiming);
                                }
                            }
                        }
                    }
                }

                await _context.SaveChangesAsync();
                tracks.Add(track);

                Console.WriteLine($"[DEBUG] Successfully synced track: {track.Title} (ID: {track.ZingMp3TrackId})");
                return tracks;
            }
            catch (JsonException ex)
            {
                Console.WriteLine($"[DEBUG] JSON parsing error: {ex.Message}");
                throw new Exception($"Lỗi phân tích JSON: {ex.Message}", ex);
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"[DEBUG] HTTP request error: {ex.Message}");
                throw new Exception($"Lỗi kết nối tới API Zing MP3: {ex.Message}", ex);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[DEBUG] General error syncing tracks: {ex.Message} - StackTrace: {ex.StackTrace}");
                throw new Exception($"Lỗi đồng bộ bài hát: {ex.Message}", ex);
            }
        }

        public async Task<List<Track>> SyncArtistSongsFromZingMp3Async(string artistId, int page = 1, int count = 5)
        {
            // Code hiện tại không đổi
            try
            {
                if (string.IsNullOrWhiteSpace(artistId))
                {
                    Console.WriteLine("[DEBUG] ArtistId is empty or null.");
                    return new List<Track>();
                }

                var artistSongsUrl = $"{_zingMp3ApiUrl}/api/artist-songs/{artistId}?page={page}&count={count}";
                Console.WriteLine($"[DEBUG] Calling Node.js API - Artist Songs: {artistSongsUrl}");

                var response = await _httpClient.GetAsync(artistSongsUrl);
                if (!response.IsSuccessStatusCode)
                {
                    Console.WriteLine($"[DEBUG] API call to /api/artist-songs failed: Status={response.StatusCode}");
                    return new List<Track>();
                }

                var json = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"[DEBUG] Raw Response from /api/artist-songs: {json}");
                var doc = JsonDocument.Parse(json);
                if (doc.RootElement.GetProperty("err").GetInt32() != 0)
                {
                    Console.WriteLine($"[DEBUG] API error for /api/artist-songs: {doc.RootElement.GetProperty("msg").GetString()}");
                    return new List<Track>();
                }

                var data = doc.RootElement.GetProperty("data");
                if (!data.TryGetProperty("items", out var itemsElement) || itemsElement.ValueKind != JsonValueKind.Array)
                {
                    Console.WriteLine("[DEBUG] No songs found for the artist.");
                    return new List<Track>();
                }

                var tracks = new List<Track>();
                foreach (var item in itemsElement.EnumerateArray())
                {
                    if (item.TryGetProperty("encodeId", out var encodeIdElement))
                    {
                        var encodeId = encodeIdElement.GetString();
                        Console.WriteLine($"[DEBUG] Syncing track with encodeId: {encodeId}");
                        var trackList = await SyncTracksFromZingMp3Async(encodeId);
                        tracks.AddRange(trackList);
                    }
                }

                Console.WriteLine($"[DEBUG] Successfully synced {tracks.Count} songs for artist ID: {artistId}");
                return tracks;
            }
            catch (JsonException ex)
            {
                Console.WriteLine($"[DEBUG] JSON parsing error: {ex.Message}");
                throw new Exception($"Lỗi phân tích JSON: {ex.Message}", ex);
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"[DEBUG] HTTP request error: {ex.Message}");
                throw new Exception($"Lỗi kết nối tới API Zing MP3: {ex.Message}", ex);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[DEBUG] General error syncing artist songs: {ex.Message} - StackTrace: {ex.StackTrace}");
                throw new Exception($"Lỗi đồng bộ bài hát của nghệ sĩ: {ex.Message}", ex);
            }
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
    }
}
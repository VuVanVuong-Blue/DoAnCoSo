using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System_Music.Models.SqlModels;
using System_Music.Models.ViewModels;
using System_Music.Services.Interfaces;
using System.Security.Claims;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;

namespace System_Music.Controllers
{
    [Authorize]
    [Route("Playlist")]
    public class PlaylistController : Controller
    {
        private readonly IPlaylistService _playlistService;
        private readonly IPlaylistTrackService _playlistTrackService;
        private readonly IUserMediaService _userMediaService;
        private readonly ILogger<PlaylistController> _logger;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ITrackService _trackService;
        private readonly IAlbumService _albumService;
        private readonly IArtistService _artistService;
        private readonly SmartMusicDbContext _context;

        public PlaylistController(
            IPlaylistService playlistService,
            IPlaylistTrackService playlistTrackService,
            IUserMediaService userMediaService,
            ITrackService trackService,
            IAlbumService albumService,
            IArtistService artistService,
            IServiceScopeFactory scopeFactory,
            SmartMusicDbContext context,
            ILogger<PlaylistController> logger)
        {
            _playlistService = playlistService;
            _playlistTrackService = playlistTrackService;
            _userMediaService = userMediaService;
            _trackService = trackService;
            _albumService = albumService;
            _artistService = artistService;
            _scopeFactory = scopeFactory;
            _context = context;
            _logger = logger;
        }

        [HttpGet]
        [Route("Index/{id}")]
        public async Task<IActionResult> Index(int id)
        {
            try
            {
                _logger.LogInformation($"Starting Index action for PlaylistId: {id}");

                var playlist = await _playlistService.GetPlaylistByIdAsync(id);
                if (playlist == null)
                {
                    _logger.LogWarning($"Playlist with ID {id} not found.");
                    return NotFound("Playlist không tồn tại!");
                }

                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                _logger.LogInformation($"UserId: {userId}, Playlist UserId: {playlist.UserId}, IsPublic: {playlist.IsPublic}");
                if (!playlist.IsPublic && playlist.UserId != userId)
                {
                    _logger.LogWarning($"User {userId} does not have permission to access Playlist {id}.");
                    return Forbid("Bạn không có quyền xem playlist này!");
                }

                _logger.LogInformation($"Fetching tracks for PlaylistId: {id}");
                var playlistTracks = await _playlistTrackService.GetTracksByPlaylistAsync(id);
                var tracks = playlistTracks
                    .Where(pt => pt != null && pt.Track != null)
                    .Select(pt => pt.Track)
                    .ToList();
                // Tải ImageMedia để tránh lỗi null
                playlist = await _context.Playlists
                    .Include(p => p.ImageMedia)
                    .FirstOrDefaultAsync(p => p.PlaylistId == id);
                var viewModel = new PlaylistViewModel
                {
                    Playlist = playlist,
                    Tracks = tracks ?? new List<Track>()
                };

                _logger.LogInformation($"Successfully prepared ViewModel for PlaylistId: {id}");
                return View("~/Views/Playlist/IndexPlayList.cshtml", viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error in Index action for PlaylistId: {id}");
                return StatusCode(500, $"Lỗi server: {ex.Message}");
            }
        }

        [HttpPost]
        [Route("UploadImage")]
        public async Task<IActionResult> UploadImage(int playlistId, IFormFile image)
        {
            try
            {
                var playlist = await _playlistService.GetPlaylistByIdAsync(playlistId);
                if (playlist == null)
                {
                    return NotFound("Playlist không tồn tại!");
                }

                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (playlist.UserId != userId)
                {
                    return Forbid("Bạn không có quyền chỉnh sửa playlist này!");
                }

                if (image == null || image.Length == 0)
                {
                    return BadRequest("Không có file ảnh được chọn!");
                }

                var uploadsDir = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads");
                if (!Directory.Exists(uploadsDir))
                {
                    Directory.CreateDirectory(uploadsDir);
                }

                var fileName = $"{playlistId}_{Guid.NewGuid()}{Path.GetExtension(image.FileName)}";
                var filePath = Path.Combine(uploadsDir, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await image.CopyToAsync(stream);
                }

                if (playlist.ImageMediaId.HasValue)
                {
                    await _userMediaService.DeleteUserMediaAsync(playlist.ImageMediaId.Value);
                }

                var userMedia = new UserMedia
                {
                    UserId = userId,
                    MediaPath = $"/uploads/{fileName}",
                    PlaylistId = playlistId,
                    UploadTime = DateTime.UtcNow
                };
                await _userMediaService.AddUserMediaAsync(userMedia);

                playlist.ImageMediaId = userMedia.MediaId;
                await _playlistService.UpdatePlaylistAsync(playlist);

                return Ok(new { ImagePath = userMedia.MediaPath });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error in UploadImage action for PlaylistId: {playlistId}");
                return BadRequest(new { message = ex.Message });
            }
        }

        [Authorize]
        [HttpPost]
        [Route("Create")]
        public async Task<IActionResult> Create([FromBody] PlaylistCreateModel request)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized("Không thể xác định người dùng.");
                }

                var playlistCount = (await _playlistService.GetUserPlaylistsAsync(userId)).Count;
                var playlist = new Playlist
                {
                    Name = string.IsNullOrEmpty(request.Name) ? $"Danh sách phát của tôi #{playlistCount + 1}" : request.Name,
                    UserId = userId,
                    CreatedDate = DateTime.UtcNow,
                    IsPublic = request.IsPublic
                };
                await _playlistService.CreatePlaylistAsync(playlist);
                return Ok(new { playlistId = playlist.PlaylistId, playlistName = playlist.Name });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in Create action");
                return BadRequest(new { message = ex.Message });
            }
        }

        [Authorize]
        [HttpPost]
        [Route("Delete/{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var playlist = await _playlistService.GetPlaylistByIdAsync(id);
                if (playlist == null)
                {
                    return NotFound("Playlist không tồn tại!");
                }

                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (playlist.UserId != userId)
                {
                    return Forbid("Bạn không có quyền xóa playlist này!");
                }

                await _playlistService.DeletePlaylistAsync(id);
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error in Delete action for PlaylistId: {id}");
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost]
        [Route("UpdatePlaylist/{id}")]
        public async Task<IActionResult> UpdatePlaylist(int id, [FromForm] string name, [FromForm] string description, [FromForm] IFormFile? image)
        {
            try
            {
                _logger.LogInformation($"Starting UpdatePlaylist action for PlaylistId: {id}");

                var playlist = await _playlistService.GetPlaylistByIdAsync(id);
                if (playlist == null)
                {
                    _logger.LogWarning($"Playlist with ID {id} not found.");
                    return NotFound("Playlist không tồn tại!");
                }

                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (playlist.UserId != userId)
                {
                    _logger.LogWarning($"User {userId} does not have permission to update Playlist {id}.");
                    return Forbid("Bạn không có quyền chỉnh sửa playlist này!");
                }

                if (!string.IsNullOrEmpty(name) && name != playlist.Name)
                {
                    playlist.Name = name;
                }

                if (description != null && description != playlist.Description)
                {
                    playlist.Description = string.IsNullOrEmpty(description) ? null : description;
                }

                if (image != null && image.Length > 0)
                {
                    var uploadsDir = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads");
                    if (!Directory.Exists(uploadsDir))
                    {
                        Directory.CreateDirectory(uploadsDir);
                    }

                    var fileName = $"{id}_{Guid.NewGuid()}{Path.GetExtension(image.FileName)}";
                    var filePath = Path.Combine(uploadsDir, fileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await image.CopyToAsync(stream);
                    }

                    if (playlist.ImageMediaId.HasValue)
                    {
                        await _userMediaService.DeleteUserMediaAsync(playlist.ImageMediaId.Value);
                    }

                    var userMedia = new UserMedia
                    {
                        UserId = userId,
                        MediaPath = $"/uploads/{fileName}",
                        PlaylistId = id,
                        UploadTime = DateTime.UtcNow
                    };
                    await _userMediaService.AddUserMediaAsync(userMedia);

                    playlist.ImageMediaId = userMedia.MediaId;
                }

                await _playlistService.UpdatePlaylistAsync(playlist);

                _logger.LogInformation($"Successfully updated PlaylistId: {id}");
                return Ok(new { message = "Playlist đã được cập nhật!", imagePath = playlist.ImageMedia?.MediaPath ?? "/images/default-playlist.png" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error in UpdatePlaylist action for PlaylistId: {id}");
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet]
        [Route("Search")]
        public async Task<IActionResult> Search(string searchTerm, int playlistId)
        {
            try
            {
                _logger.LogInformation($"Starting Search action with searchTerm: {searchTerm}, PlaylistId: {playlistId}");

                var playlist = await _playlistService.GetPlaylistByIdAsync(playlistId);
                if (playlist == null)
                {
                    _logger.LogWarning($"Playlist with ID {playlistId} not found.");
                    return NotFound("Playlist không tồn tại!");
                }

                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (!playlist.IsPublic && playlist.UserId != userId)
                {
                    _logger.LogWarning($"User {userId} does not have permission to access Playlist {playlistId}.");
                    return Forbid("Bạn không có quyền truy cập playlist này!");
                }

                var songsTask = Task.Run(async () =>
                {
                    using var scope = _scopeFactory.CreateScope();
                    var trackService = scope.ServiceProvider.GetRequiredService<ITrackService>();
                    return await trackService.SearchTracksAsync(searchTerm) ?? new List<Track>();
                });

                var albumsTask = Task.Run(async () =>
                {
                    using var scope = _scopeFactory.CreateScope();
                    var albumService = scope.ServiceProvider.GetRequiredService<IAlbumService>();
                    return await albumService.SearchAlbumsAsync(searchTerm) ?? new List<Album>();
                });

                var artistsTask = Task.Run(async () =>
                {
                    using var scope = _scopeFactory.CreateScope();
                    var artistService = scope.ServiceProvider.GetRequiredService<IArtistService>();
                    return await artistService.SearchArtistsAsync(searchTerm) ?? new List<Artist>();
                });

                await Task.WhenAll(songsTask, albumsTask, artistsTask);

                var songs = songsTask.Result;
                var albums = albumsTask.Result;
                var artists = artistsTask.Result;

                var songsResult = songs.Select(s => new
                {
                    id = s.TrackId,
                    title = s.Title,
                    artist = string.Join(", ", s.TrackArtists?.Select(ta => ta.Artist?.Name ?? "Unknown Artist")?.ToList() ?? new List<string>()),
                    imageUrl = s.ImageUrl ?? s.Album?.Image ?? "/images/default-track.png"
                }).ToList();

                var albumsResult = albums.Select(a => new
                {
                    id = a.AlbumId,
                    name = a.Name,
                    artist = string.Join(", ", a.AlbumArtists?.Select(aa => aa.Artist?.Name ?? "Unknown Artist")?.ToList() ?? new List<string>()),
                    imageUrl = a.Image ?? "/images/default-album.png"
                }).ToList();

                var artistsResult = artists.Select(a => new
                {
                    id = a.ArtistId,
                    name = a.Name ?? "Unknown Artist",
                    imageUrl = a.Image ?? "/images/default-artist.png"
                }).ToList();

                var result = new
                {
                    songs = songsResult,
                    albums = albumsResult,
                    artists = artistsResult
                };

                _logger.LogInformation($"Search completed successfully for searchTerm: {searchTerm}");
                return Json(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error in Search action with searchTerm: {searchTerm}, PlaylistId: {playlistId}");
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [HttpGet]
        [Route("GetAlbumSongs")]
        public async Task<IActionResult> GetAlbumSongs(int albumId)
        {
            try
            {
                _logger.LogInformation($"Starting GetAlbumSongs action for AlbumId: {albumId}");

                var album = await _context.Albums
                    .Include(a => a.Tracks)
                    .ThenInclude(t => t.TrackArtists)
                    .ThenInclude(ta => ta.Artist)
                    .FirstOrDefaultAsync(a => a.AlbumId == albumId);

                if (album == null)
                {
                    _logger.LogWarning($"Album with ID {albumId} not found.");
                    return NotFound("Album không tồn tại!");
                }

                // Kiểm tra Tracks để tránh NullReferenceException
                var tracks = album.Tracks ?? new List<Track>();

                var songs = tracks.Select(t => new
                {
                    id = t.TrackId,
                    title = t.Title,
                    artist = string.Join(", ", t.TrackArtists?.Select(ta => ta.Artist?.Name ?? "Unknown Artist")?.ToList() ?? new List<string>()),
                    imageUrl = t.ImageUrl ?? album.Image ?? "/images/default-track.png"
                }).ToList();

                _logger.LogInformation($"Found {songs.Count} songs for AlbumId: {albumId}");
                return Json(songs);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error in GetAlbumSongs action for AlbumId: {albumId}");
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [HttpGet]
        [Route("ProxyImage")]
        public async Task<IActionResult> ProxyImage(string url)
        {
            try
            {
                if (string.IsNullOrEmpty(url))
                {
                    _logger.LogWarning("ProxyImage received an empty URL.");
                    return NotFound("URL không hợp lệ!");
                }

                if (url.StartsWith("/"))
                {
                    return Ok(new { imageUrl = url });
                }

                using var httpClient = new HttpClient
                {
                    Timeout = TimeSpan.FromSeconds(10) // Thêm timeout 10 giây
                };
                var response = await httpClient.GetAsync(url);
                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning($"Failed to fetch image from URL: {url}, Status: {response.StatusCode}");
                    return ReturnDefaultImage();
                }

                var content = await response.Content.ReadAsByteArrayAsync();
                var contentType = response.Content.Headers.ContentType?.ToString() ?? "image/jpeg";
                return File(content, contentType);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error in ProxyImage for URL: {url}");
                return ReturnDefaultImage();
            }
        }

        private IActionResult ReturnDefaultImage()
        {
            var defaultImagePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images/default-image.png");
            if (!System.IO.File.Exists(defaultImagePath))
            {
                _logger.LogWarning("Default image not found at: {defaultImagePath}", defaultImagePath);
                return NotFound("Không tìm thấy hình ảnh mặc định!");
            }

            var fileInfo = new System.IO.FileInfo(defaultImagePath);
            string contentType = fileInfo.Extension.ToLower() switch
            {
                ".png" => "image/png",
                ".jpg" => "image/jpeg",
                ".jpeg" => "image/jpeg",
                ".gif" => "image/gif",
                _ => "application/octet-stream"
            };

            var defaultImageContent = System.IO.File.ReadAllBytes(defaultImagePath);
            return File(defaultImageContent, contentType);
        }

        [HttpGet]
        [Route("SearchUserPlaylists")]
        public async Task<IActionResult> SearchUserPlaylists(string searchText)
        {
            try
            {
                _logger.LogInformation($"Starting SearchUserPlaylists action with searchText: {searchText}");

                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                {
                    _logger.LogWarning("User not authenticated.");
                    return Unauthorized("Không thể xác định người dùng.");
                }

                var playlists = await _context.Playlists
                    .Include(p => p.ImageMedia)
                    .Include(p => p.PlaylistTracks)
                        .ThenInclude(pt => pt.Track)
                    .Where(p => p.UserId == userId)
                    .ToListAsync();

                if (string.IsNullOrEmpty(searchText))
                {
                    _logger.LogInformation($"No search text provided, returning all playlists for user {userId}.");
                    return Ok(playlists.Select(p => new
                    {
                        p.PlaylistId,
                        p.Name,
                        ImagePath = p.ImageMedia?.MediaPath,
                        p.UserId
                    }));
                }

                searchText = searchText.ToLower();
                var filteredPlaylists = playlists
                    .Where(p => (p.Name != null && p.Name.ToLower().Contains(searchText)) ||
                                (p.Description != null && p.Description.ToLower().Contains(searchText)) ||
                                p.PlaylistTracks.Any(pt => pt.Track != null && pt.Track.Title != null && pt.Track.Title.ToLower().Contains(searchText)))
                    .Select(p => new
                    {
                        p.PlaylistId,
                        p.Name,
                        ImagePath = p.ImageMedia?.MediaPath,
                        p.UserId
                    })
                    .ToList();

                _logger.LogInformation($"Found {filteredPlaylists.Count} playlists matching searchText: {searchText}");
                return Ok(filteredPlaylists);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error in SearchUserPlaylists action with searchText: {searchText}");
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [HttpPost]
        [Route("AddSong")]
        public async Task<IActionResult> AddSong(int playlistId, int songId)
        {
            try
            {
                var playlist = await _playlistService.GetPlaylistByIdAsync(playlistId);
                if (playlist == null)
                {
                    return NotFound("Playlist không tồn tại!");
                }

                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (playlist.UserId != userId)
                {
                    return Forbid("Bạn không có quyền chỉnh sửa playlist này!");
                }

                var track = await _context.Tracks.FindAsync(songId);
                if (track == null)
                {
                    return NotFound("Bài hát không tồn tại!");
                }

                var existingTrack = await _context.PlaylistTracks
                    .FirstOrDefaultAsync(pt => pt.PlaylistId == playlistId && pt.TrackId == songId);
                if (existingTrack != null)
                {
                    return BadRequest("Bài hát đã có trong playlist!");
                }

                var playlistTrack = new PlaylistTrack
                {
                    PlaylistId = playlistId,
                    TrackId = songId,
                    AddedDate = DateTime.UtcNow
                };
                await _playlistTrackService.AddPlaylistTrackAsync(playlistTrack);

                return Ok(new { message = "Bài hát đã được thêm vào playlist!" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error in AddSong action for PlaylistId: {playlistId}, SongId: {songId}");
                return BadRequest(new { message = ex.Message });
            }
        }
        [HttpGet]
        [Route("GetArtistDetails")]
        public async Task<IActionResult> GetArtistDetails(int artistId)
        {
            try
            {
                _logger.LogInformation($"Starting GetArtistDetails action for ArtistId: {artistId}");

                // Lấy danh sách bài hát của nghệ sĩ
                var songs = await _context.Tracks
                    .Include(t => t.TrackArtists)
                    .ThenInclude(ta => ta.Artist)
                    .Where(t => t.TrackArtists.Any(ta => ta.ArtistId == artistId))
                    .Take(5) // Lấy tối đa 5 bài hát cho phần "Những bài hát được ưa chuộng"
                    .ToListAsync();

                // Lấy danh sách album của nghệ sĩ
                var albums = await _context.Albums
                    .Include(a => a.AlbumArtists)
                    .ThenInclude(aa => aa.Artist)
                    .Where(a => a.AlbumArtists.Any(aa => aa.ArtistId == artistId))
                    .Take(5) // Lấy tối đa 5 album
                    .ToListAsync();

                // Chuyển đổi dữ liệu thành định dạng JSON
                var songsResult = songs.Select(s => new
                {
                    id = s.TrackId,
                    title = s.Title,
                    artist = string.Join(", ", s.TrackArtists?.Select(ta => ta.Artist?.Name ?? "Unknown Artist")?.ToList() ?? new List<string>()),
                    imageUrl = s.ImageUrl ?? s.Album?.Image ?? "/images/default-track.png"
                }).ToList();

                var albumsResult = albums.Select(a => new
                {
                    id = a.AlbumId,
                    name = a.Name,
                    artist = string.Join(", ", a.AlbumArtists?.Select(aa => aa.Artist?.Name ?? "Unknown Artist")?.ToList() ?? new List<string>()),
                    imageUrl = a.Image ?? "/images/default-album.png"
                }).ToList();

                var result = new
                {
                    songs = songsResult,
                    albums = albumsResult
                };

                _logger.LogInformation($"Found {songsResult.Count} songs and {albumsResult.Count} albums for ArtistId: {artistId}");
                return Json(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error in GetArtistDetails action for ArtistId: {artistId}");
                return StatusCode(500, new { message = ex.Message });
            }
        }
        [HttpPost]
        [Route("RemoveSong")]
        public async Task<IActionResult> RemoveSong(int playlistId, int songId)
        {
            try
            {
                _logger.LogInformation($"Starting RemoveSong action for PlaylistId: {playlistId}, SongId: {songId}");

                // Kiểm tra playlist có tồn tại không
                var playlist = await _playlistService.GetPlaylistByIdAsync(playlistId);
                if (playlist == null)
                {
                    _logger.LogWarning($"Playlist with ID {playlistId} not found.");
                    return NotFound("Playlist không tồn tại!");
                }

                // Kiểm tra quyền của người dùng
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (playlist.UserId != userId)
                {
                    _logger.LogWarning($"User {userId} does not have permission to modify Playlist {playlistId}.");
                    return Forbid("Bạn không có quyền chỉnh sửa playlist này!");
                }

                // Tìm bản ghi PlaylistTrack cần xóa
                var playlistTrack = await _context.PlaylistTracks
                    .FirstOrDefaultAsync(pt => pt.PlaylistId == playlistId && pt.TrackId == songId);

                if (playlistTrack == null)
                {
                    _logger.LogWarning($"Track with ID {songId} not found in Playlist {playlistId}.");
                    return NotFound("Bài hát không có trong playlist!");
                }

                // Xóa bản ghi PlaylistTrack
                _context.PlaylistTracks.Remove(playlistTrack);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"Successfully removed SongId: {songId} from PlaylistId: {playlistId}");
                return Ok(new { message = "Bài hát đã được xóa khỏi playlist!" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error in RemoveSong action for PlaylistId: {playlistId}, SongId: {songId}");
                return BadRequest(new { message = ex.Message });
            }
        }

        // Model để nhận dữ liệu từ request
        public class RemoveSongsRequest
        {
            public int PlaylistId { get; set; }
            public List<int> SongIds { get; set; }
        }

        [HttpGet]
        [Route("GetTracks/{playlistId}")]
        public async Task<IActionResult> GetTracks(int playlistId)
        {
            try
            {
                _logger.LogInformation($"Starting GetTracks action for PlaylistId: {playlistId}");

                var playlist = await _playlistService.GetPlaylistByIdAsync(playlistId);
                if (playlist == null)
                {
                    _logger.LogWarning($"Playlist with ID {playlistId} not found.");
                    return NotFound("Playlist không tồn tại!");
                }

                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (!playlist.IsPublic && playlist.UserId != userId)
                {
                    _logger.LogWarning($"User {userId} does not have permission to access Playlist {playlistId}.");
                    return Forbid("Bạn không có quyền truy cập playlist này!");
                }

                var tracks = await _context.PlaylistTracks
                    .Where(pt => pt.PlaylistId == playlistId)
                    .Include(pt => pt.Track)
                        .ThenInclude(t => t.TrackArtists)
                            .ThenInclude(ta => ta.Artist)
                    .Include(pt => pt.Track)
                        .ThenInclude(t => t.Album)
                    .Select(pt => new
                    {
                        id = pt.TrackId,
                        title = pt.Track.Title,
                        duration = pt.Track.Duration,
                        audioUrl = pt.Track.AudioUrl ?? "",
                        imageUrl = pt.Track.ImageUrl ?? pt.Track.Album.Image ?? "/images/default-track.png",
                        trackArtists = pt.Track.TrackArtists.Select(ta => new
                        {
                            artist = new
                            {
                                id = ta.ArtistId,
                                name = ta.Artist.Name ?? "Unknown"
                            }
                        }).ToList(), // Đổi từ "artists" thành "trackArtists" và định dạng đúng
                        album = pt.Track.Album != null ? new
                        {
                            id = pt.Track.Album.AlbumId,
                            name = pt.Track.Album.Name
                        } : null,
                        addedDate = pt.AddedDate.ToString("o")
                    })
                    .ToListAsync();

                _logger.LogInformation($"Found {tracks.Count} tracks for PlaylistId: {playlistId}");
                return Ok(tracks);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error in GetTracks action for PlaylistId: {playlistId}");
                return StatusCode(500, new { message = ex.Message });
            }
        }
    }
}
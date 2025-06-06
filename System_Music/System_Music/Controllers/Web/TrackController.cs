using Microsoft.AspNetCore.Mvc;
using System_Music.Services.Interfaces;
using System_Music.Models.SqlModels;
using System.Threading.Tasks;
using System_Music.Models.ViewModels;
using System.Security.Claims;
using System_Music.Services.Implementations;
using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace System_Music.Controllers
{
    public class TrackController : Controller
    {
        private readonly ITrackService _trackService;
        private readonly IListenHistoryService _listenHistoryService;
        private readonly SmartMusicDbContext _context;

        public TrackController(ITrackService trackService, IListenHistoryService listenHistoryService, SmartMusicDbContext context)
        {
            _trackService = trackService;
            _listenHistoryService = listenHistoryService;
            _context = context;
        }

        // API endpoint để lấy thông tin bài hát theo trackId  
        [HttpGet("api/track/{id}")]
        public async Task<IActionResult> GetTrack(int id)
        {
            var track = await _trackService.GetTrackByIdAsync(id);
            if (track == null)
            {
                return NotFound("Không tìm thấy bài hát");
            }

            // Trả về dữ liệu bài hát dưới dạng JSON  
            return Ok(new
            {
                trackId = track.TrackId,
                title = track.Title,
                artists = track.TrackArtists?.Select(ta => ta.Artist?.Name).Where(name => !string.IsNullOrEmpty(name)).ToList() ?? new List<string>(),
                imageUrl = track.ImageUrl,
                audioUrl = track.AudioUrl,
                duration = track.Duration
            });
        }

        [HttpGet]
        [Route("Track/GetCurrentTrack")]
        public async Task<IActionResult> GetCurrentTrack(int? trackId = null)
        {
            var tracks = await _trackService.GetAllTracksAsync();
            if (tracks == null || !tracks.Any())
            {
                return NotFound("Không có bài hát nào trong hệ thống.");
            }

            var track = tracks.FirstOrDefault(t => t.TrackId == trackId) ?? tracks.First();
            return Ok(new
            {
                trackId = track.TrackId,
                title = track.Title,
                artists = track.TrackArtists?.Select(ta => ta.Artist?.Name).Where(name => !string.IsNullOrEmpty(name)).ToList() ?? new List<string>(),
                imageUrl = track.ImageUrl,
                audioUrl = track.AudioUrl,
                duration = track.Duration
            });
        }

        [HttpGet]
        [Route("Track/Detail/{id}")]
        public async Task<IActionResult> Detail(int id)
        {
            var track = await _trackService.GetTrackByIdAsync(id);
            if (track == null)
            {
                return NotFound();
            }
            Console.WriteLine($"TrackController - Track Data: TrackId={track.TrackId}, Title={track.Title}, AudioUrl={(track.AudioUrl ?? "null")}");
            var viewModel = new TrackDetailViewModel { Track = track };
            return View("~/Views/Playlist/TrackDetail.cshtml", viewModel);
        }

        [HttpPost]
        [Route("api/track/AddListenHistory")]
        public async Task<IActionResult> AddListenHistory([FromBody] AddListenHistoryRequest request)
        {
            if (request == null || request.TrackId <= 0)
            {
                return BadRequest("TrackId không hợp lệ.");
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "defaultUserId";
            Console.WriteLine($"UserId: {userId}, TrackId: {request.TrackId}");

            var track = await _trackService.GetTrackByIdAsync(request.TrackId);
            if (track == null)
            {
                return NotFound("Không tìm thấy bài hát.");
            }

            var listenHistory = new ListenHistory
            {
                UserId = userId,
                TrackId = request.TrackId,
                ListenDate = DateTime.UtcNow
            };

            try
            {
                await _listenHistoryService.AddListenHistoryAsync(listenHistory);
                return Ok("Đã lưu lịch sử nghe.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi khi lưu lịch sử nghe: {ex.Message}");
                return StatusCode(500, "Lỗi server khi lưu lịch sử nghe.");
            }
        }

        public class AddListenHistoryRequest
        {
            public int TrackId { get; set; }
        }

        [HttpGet]
        [Route("Track/Search")]
        public async Task<IActionResult> Search(string query)
        {
            try
            {
                if (string.IsNullOrEmpty(query))
                {
                    return Json(new { tracks = new List<object>(), artists = new List<object>() });
                }

                // Tìm kiếm bài hát theo tiêu đề
                var tracks = await _context.Tracks
                    .Include(t => t.TrackArtists)
                        .ThenInclude(ta => ta.Artist)
                    .Include(t => t.Album)
                    .Where(t => t.Title.Contains(query))
                    .Select(t => new
                    {
                        id = t.TrackId,
                        title = t.Title,
                        subtitle = t.TrackArtists.Any() ? t.TrackArtists.FirstOrDefault().Artist.Name ?? "Unknown Artist" : "Unknown Artist",
                        image = t.ImageUrl != null ? t.ImageUrl : (t.Album != null ? t.Album.Image : "/images/default-track.png"),
                        url = $"/track/{t.TrackId}"
                    })
                    .Take(5)
                    .ToListAsync();

                // Tìm kiếm nghệ sĩ theo tên
                var artists = await _context.Artists
                    .Where(a => a.Name.Contains(query))
                    .Select(a => new
                    {
                        id = a.ArtistId,
                        title = a.Name,
                        subtitle = "Nghệ sĩ",
                        image = a.Image ?? "/images/default-artist.png",
                        url = $"/artist/{a.ArtistId}"
                    })
                    .Take(5)
                    .ToListAsync();

                // Tìm kiếm album theo tên
                var albums = await _context.Albums
                    .Include(a => a.AlbumArtists)
                        .ThenInclude(aa => aa.Artist)
                    .Where(a => a.Name.Contains(query))
                    .Select(a => new
                    {
                        id = a.AlbumId,
                        title = a.Name,
                        subtitle = a.AlbumArtists.Any() ? a.AlbumArtists.FirstOrDefault().Artist.Name ?? "Unknown Artist" : "Unknown Artist",
                        image = a.Image ?? "/images/default-album.png",
                        url = $"/album/{a.AlbumId}"
                    })
                    .Take(5)
                    .ToListAsync();

                var results = new
                {
                    tracks,
                    artists,
                    albums
                };

                return Json(results);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in Search: {ex.Message}");
                return StatusCode(500, new { error = "Internal Server Error" });
            }
        }
    }
}

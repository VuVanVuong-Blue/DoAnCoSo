using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System_Music.Interfaces;
using System.Threading.Tasks;
using System_Music.Models.SqlModels;

namespace System_Music.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class VideosController : Controller
    {
        private readonly IVideoService _videoService;

        public VideosController(IVideoService videoService)
        {
            _videoService = videoService ?? throw new ArgumentNullException(nameof(videoService));
        }

        public async Task<IActionResult> Index()
        {
            var videos = await _videoService.GetAllVideosAsync();
            if (videos == null)
            {
                Console.WriteLine("GetAllVideosAsync returned null");
                videos = Enumerable.Empty<Video>();
            }
            Console.WriteLine($"Retrieved {videos.Count()} videos");
            return View(videos);
        }

        public async Task<IActionResult> Details(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }

            var video = await _videoService.GetVideoByIdAsync(id);
            if (video == null)
            {
                return NotFound();
            }
            Console.WriteLine($"Hls URL for video {id}: {video.Hls}");
            return View(video);
        }

        // Trang thêm mới
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Video video)
        {
            if (ModelState.IsValid)
            {
                // Lưu video vào database (có thể cần xử lý thêm VideoArtist)
                await _videoService.AddVideoAsync(video);
                return RedirectToAction(nameof(Index));
            }
            return View(video);
        }

        // Trang sửa
        public async Task<IActionResult> Edit(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }

            var video = await _videoService.GetVideoByIdAsync(id);
            if (video == null)
            {
                return NotFound();
            }

            return View(video);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, Video video)
        {
            if (id != video.EncodeId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                await _videoService.UpdateVideoAsync(video); // Giả định phương thức này
                return RedirectToAction(nameof(Index));
            }
            return View(video);
        }

        // Trang xóa
        public async Task<IActionResult> Delete(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }

            var video = await _videoService.GetVideoByIdAsync(id);
            if (video == null)
            {
                return NotFound();
            }

            return View(video);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            await _videoService.DeleteVideoAsync(id); // Giả định phương thức này
            return RedirectToAction(nameof(Index));
        }

        // Thêm nút lấy MV từ Zing MP3
        public async Task<IActionResult> FetchFromZingMp3(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return BadRequest("ID không được để trống");
            }

            try
            {
                var video = await _videoService.GetVideoByIdAsync(id);
                if (video == null)
                {
                    video = await _videoService.GetVideoByIdAsync(id); // Gọi API và lưu vào database
                }
                return RedirectToAction(nameof(Details), new { id = video.EncodeId });
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Lỗi khi lấy MV: {ex.Message}");
                return View("Index");
            }
        }
        public async Task<IActionResult> VideoMV(string videoId)
        {
            var video = await _videoService.GetVideoByIdAsync(videoId);
            if (video == null) return NotFound();

            var relatedVideos = await _videoService.GetRelatedVideosAsync(videoId);
            ViewBag.RelatedVideos = relatedVideos;

            return View(video);
        }
    }
}
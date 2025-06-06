using Microsoft.AspNetCore.Mvc;
using System_Music.Interfaces;
using System_Music.Services.Implementations;
using System_Music.Services.Interfaces;

namespace System_Music.Controllers
{
    public class HomeController : Controller
    {
        private readonly IAlbumService _albumService;
        private readonly IPlaylistService _playlistService;
        private readonly IArtistService _artistService;
        private readonly IVideoService _videoService;
        public HomeController(IAlbumService albumService, IPlaylistService playlistService, IArtistService artistService, IVideoService videoService)
        {
            _albumService = albumService;
            _playlistService = playlistService;
            _artistService = artistService;
            _videoService = videoService ?? throw new ArgumentNullException(nameof(videoService));
        }

        public IActionResult Index()
        {
            return View();
        }

        public async Task<IActionResult> DetailAlbum(int id)
        {
            var album = await _albumService.GetAlbumByIdWithDetailsAsync(id);
            if (album == null)
            {
                return NotFound();
            }
            return View(album);
        }
        public IActionResult Premium()
        {
            return View("~/Views/Premium/Premium.cshtml");
        }
        // Action mới để hiển thị tất cả nghệ sĩ
        public async Task<IActionResult> ArtistsPopular()
        {
            var artists = await _artistService.GetAllArtistsAsync();
            return View(artists);
        }
        [Route("Video/VideoMV")]
        public async Task<IActionResult> VideoMV(string videoId)
        {
            var video = await _videoService.GetVideoByIdAsync(videoId);
            if (video == null) return NotFound();

            var relatedVideos = await _videoService.GetRelatedVideosAsync(videoId);
            ViewBag.RelatedVideos = relatedVideos;

            return View("~/Views/Video/VideoMV.cshtml", video);

        }
    }
}
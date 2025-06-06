using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System_Music.Services.Interfaces;

namespace System_Music.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class DashboardController : Controller
    {
        private readonly IUserService _userService;
        private readonly ITrackService _trackService;
        private readonly IAlbumService _albumService;
        private readonly IArtistService _artistService;

        public DashboardController(
            IUserService userService,
            ITrackService trackService,
            IAlbumService albumService,
            IArtistService artistService)
        {
            _userService = userService;
            _trackService = trackService;
            _albumService = albumService;
            _artistService = artistService;
        }

        public async Task<IActionResult> Index()
        {
            ViewBag.UserCount = (await _userService.GetAllUsersAsync()).Count;
            ViewBag.TrackCount = (await _trackService.GetAllTracksAsync()).Count;
            ViewBag.AlbumCount = (await _albumService.GetAllAlbumsAsync()).Count;
            ViewBag.ArtistCount = (await _artistService.GetAllArtistsAsync()).Count;
            ViewBag.TopTracks = await _trackService.GetTopTracksAsync(5);

            return View();
        }
    }
}
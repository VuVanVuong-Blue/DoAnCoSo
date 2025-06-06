using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System_Music.Services.Interfaces;

namespace System_Music.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class LikeController : Controller
    {
        private readonly ILikeTrackService _likeTrackService;


        public LikeController(ILikeTrackService likeTrackService)
        {
            _likeTrackService = likeTrackService;
       
        }

        // Quản lý LikeTrack
        public async Task<IActionResult> TrackLikes()
        {
            var trackLikes = await _likeTrackService.GetAllLikesAsync();
            return View(trackLikes);
        }

        public async Task<IActionResult> DeleteTrackLike(int id)
        {
            await _likeTrackService.DeleteLikeAsync(id);
            return RedirectToAction(nameof(TrackLikes));
        }

    }
}
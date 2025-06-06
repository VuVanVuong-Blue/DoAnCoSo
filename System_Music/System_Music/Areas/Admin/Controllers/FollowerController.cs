using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System_Music.Services.Interfaces;

namespace System_Music.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class FollowerController : Controller
    {
        private readonly IFollowerService _followerService;

        public FollowerController(IFollowerService followerService)
        {
            _followerService = followerService;
        }

        public async Task<IActionResult> Index()
        {
            var followers = await _followerService.GetAllFollowersAsync();
            return View(followers);
        }

        public async Task<IActionResult> Delete(int id)
        {
            await _followerService.DeleteFollowerAsync(id);
            return RedirectToAction(nameof(Index));
        }
    }
}
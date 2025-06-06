using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System_Music.Models.SqlModels;
using System_Music.Services.Interfaces;

namespace System_Music.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class LyricsTimingController : Controller
    {
        private readonly ILyricsTimingService _lyricsTimingService;

        public LyricsTimingController(ILyricsTimingService lyricsTimingService)
        {
            _lyricsTimingService = lyricsTimingService;
        }

        public async Task<IActionResult> Index()
        {
            var lyrics = await _lyricsTimingService.GetAllLyricsAsync();
            return View(lyrics);
        }

        public async Task<IActionResult> Details(int id)
        {
            var lyric = await _lyricsTimingService.GetLyricByIdAsync(id);
            if (lyric == null)
            {
                return NotFound();
            }
            return View(lyric);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(LyricsTiming lyricsTiming)
        {
            if (ModelState.IsValid)
            {
                await _lyricsTimingService.AddLyricAsync(lyricsTiming);
                return RedirectToAction(nameof(Index));
            }
            return View(lyricsTiming);
        }

        public async Task<IActionResult> Edit(int id)
        {
            var lyric = await _lyricsTimingService.GetLyricByIdAsync(id);
            if (lyric == null)
            {
                return NotFound();
            }
            return View(lyric);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, LyricsTiming lyricsTiming)
        {
            if (id != lyricsTiming.LyricsTimingId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                await _lyricsTimingService.UpdateLyricAsync(lyricsTiming);
                return RedirectToAction(nameof(Index));
            }
            return View(lyricsTiming);
        }

        public async Task<IActionResult> Delete(int id)
        {
            var lyric = await _lyricsTimingService.GetLyricByIdAsync(id);
            if (lyric == null)
            {
                return NotFound();
            }
            return View(lyric);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await _lyricsTimingService.DeleteLyricAsync(id);
            return RedirectToAction(nameof(Index));
        }
    }
}
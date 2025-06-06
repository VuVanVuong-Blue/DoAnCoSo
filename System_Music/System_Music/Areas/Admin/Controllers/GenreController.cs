using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System_Music.Models.SqlModels;
using System_Music.Services.Interfaces;

namespace System_Music.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Policy = "Admin")] // Sử dụng chính sách "Admin" thay vì Roles
    public class GenreController : Controller
    {
        private readonly IGenreService _genreService;

        public GenreController(IGenreService genreService)
        {
            _genreService = genreService;
        }

        public async Task<IActionResult> Index()
        {
            var genres = await _genreService.GetAllGenresAsync();
            return View(genres);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Genre genre)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    await _genreService.AddGenreAsync(genre);
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError(string.Empty, ex.Message);
                }
            }
            return View(genre);
        }

        public async Task<IActionResult> Edit(int id)
        {
            try
            {
                var genre = await _genreService.GetGenreByIdAsync(id);
                return View(genre);
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Genre genre)
        {
            Console.WriteLine($"[DEBUG] Edit POST được gọi với id = {id}, genre.GenreId = {genre.GenreId}, Name = {genre.Name}");

            if (id != genre.GenreId)
            {
                Console.WriteLine("[DEBUG] ID không khớp, trả về NotFound()");
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    Console.WriteLine($"[DEBUG] Updating Genre: {genre.Description}");
                    await _genreService.UpdateGenreAsync(genre);
                    Console.WriteLine("[DEBUG] Gọi UpdateGenreAsync thành công");
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[ERROR] Lỗi khi gọi UpdateGenreAsync: {ex.Message}");
                    ModelState.AddModelError(string.Empty, ex.Message);
                }
            }
            else
            {
                Console.WriteLine("[DEBUG] ModelState không hợp lệ");
            }

            return View(genre);
        }

        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var genre = await _genreService.GetGenreByIdAsync(id);
                return View(genre);
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                await _genreService.DeleteGenreAsync(id);
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                var genre = await _genreService.GetGenreByIdAsync(id);
                return View(genre);
            }
        }

        public async Task<IActionResult> Details(int id)
        {
            try
            {
                var genre = await _genreService.GetGenreByIdAsync(id);
                return View(genre);
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
        }

        [HttpGet]
        public async Task<IActionResult> SyncFromZingMp3(bool refresh = false)
        {
            try
            {
                IEnumerable<Genre> genres;
                if (refresh)
                {
                    genres = await _genreService.SyncGenresFromZingMp3Async();
                }
                else
                {
                    genres = await _genreService.GetAllGenresAsync();
                }
                return View(genres);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] Error syncing genres from Zing MP3: {ex.Message}");
                TempData["Error"] = "Failed to sync genres from Zing MP3. Please try again later.";
                return RedirectToAction(nameof(Index));
            }
        }
    }
}
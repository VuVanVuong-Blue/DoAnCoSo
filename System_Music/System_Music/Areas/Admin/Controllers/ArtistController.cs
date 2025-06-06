using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System_Music.Models.SqlModels;
using System_Music.Services.Interfaces;

namespace System_Music.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class ArtistController : Controller
    {
        private readonly IArtistService _artistService;

        public ArtistController(IArtistService artistService)
        {
            _artistService = artistService;
        }

        public async Task<IActionResult> Index()
        {
            var artists = await _artistService.GetAllArtistsAsync();
            return View(artists);
        }

        public async Task<IActionResult> Details(int id)
        {
            var artist = await _artistService.GetArtistByIdAsync(id);
            if (artist == null)
            {
                return NotFound();
            }
            return View(artist);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Artist artist, IFormFile? ImageFile, string ImageUrl)
        {
            Console.WriteLine($"Artist name: {artist.Name}");
            Console.WriteLine($"ImageUrl: {ImageUrl}");
            Console.WriteLine($"ImageFile: {(ImageFile != null ? ImageFile.FileName : "NULL")}");

            if (!string.IsNullOrWhiteSpace(ImageUrl))
            {
                artist.Image = ImageUrl;
            }
            else if (ImageFile != null && ImageFile.Length > 0)
            {
                var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images/artists");
                Console.WriteLine($"Upload folder: {uploadsFolder}");

                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }

                var fileName = $"{Guid.NewGuid()}_{Path.GetFileName(ImageFile.FileName)}";
                var filePath = Path.Combine(uploadsFolder, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await ImageFile.CopyToAsync(stream);
                }

                artist.Image = $"/images/artists/{fileName}";
            }

            if (ModelState.IsValid)
            {
                await _artistService.AddArtistAsync(artist);
                return RedirectToAction(nameof(Index));
            }

            foreach (var modelState in ModelState)
            {
                foreach (var error in modelState.Value.Errors)
                {
                    Console.WriteLine($"ModelState error in {modelState.Key}: {error.ErrorMessage}");
                }
            }

            return View(artist);
        }

        public async Task<IActionResult> Edit(int id)
        {
            var artist = await _artistService.GetArtistByIdAsync(id);
            if (artist == null)
            {
                return NotFound();
            }
            return View(artist);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Artist artist)
        {
            if (id != artist.ArtistId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                await _artistService.UpdateArtistAsync(artist);
                return RedirectToAction(nameof(Index));
            }
            return View(artist);
        }

        public async Task<IActionResult> Delete(int id)
        {
            var artist = await _artistService.GetArtistByIdAsync(id);
            if (artist == null)
            {
                return NotFound();
            }
            return View(artist);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await _artistService.DeleteArtistAsync(id);
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> SyncFromZingMp3(string artistName = null, string artistId = null)
        {
            try
            {
                IEnumerable<Artist> artists;
                if (!string.IsNullOrEmpty(artistName) || !string.IsNullOrEmpty(artistId))
                {
                    ViewData["ArtistName"] = artistName;
                    ViewData["ArtistId"] = artistId;
                    artists = await _artistService.SyncArtistsFromZingMp3Async(artistName, artistId);
                }
                else
                {
                    artists = await _artistService.GetAllArtistsAsync();
                }
                return View(artists);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] Error syncing artists from Zing MP3: {ex.Message}");
                TempData["Error"] = "Failed to sync artists from Zing MP3. Please try again later.";
                return RedirectToAction(nameof(Index));
            }
        }
    }
}
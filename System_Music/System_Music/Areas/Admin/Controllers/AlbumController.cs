using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System_Music.Models.SqlModels;
using System_Music.Services.Interfaces;

namespace System_Music.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class AlbumController : Controller
    {
        private readonly IAlbumService _albumService;
        private readonly IArtistService _artistService;
        private readonly SmartMusicDbContext _context;

        public AlbumController(IAlbumService albumService, IArtistService artistService, SmartMusicDbContext context)
        {
            _albumService = albumService;
            _artistService = artistService;
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var albums = await _albumService.GetAllAlbumsAsync();
            return View(albums);
        }

        public async Task<IActionResult> Details(int id)
        {
            var album = await _albumService.GetAlbumByIdWithDetailsAsync(id);
            if (album == null)
            {
                return NotFound();
            }
            return View(album);
        }

        public async Task<IActionResult> Create()
        {
            ViewBag.Artists = await _artistService.GetAllArtistsAsync();
            ViewBag.Tracks = await _context.Tracks.ToListAsync();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Album album, IFormFile? imageFile, int[] artistIds, int[] trackIds)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    if (imageFile != null && imageFile.Length > 0)
                    {
                        var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images/albums");
                        if (!Directory.Exists(uploadsFolder))
                        {
                            Directory.CreateDirectory(uploadsFolder);
                        }

                        var fileName = $"{Guid.NewGuid()}_{Path.GetFileName(imageFile.FileName)}";
                        var filePath = Path.Combine(uploadsFolder, fileName);

                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            await imageFile.CopyToAsync(stream);
                        }

                        album.Image = $"/images/albums/{fileName}";
                    }

                    await _albumService.AddAlbumAsync(album, artistIds, trackIds);
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", $"Lỗi khi tạo album: {ex.Message}");
                    if (ex.InnerException != null)
                    {
                        ModelState.AddModelError("", $"Chi tiết lỗi: {ex.InnerException.Message}");
                    }
                }
            }

            ViewBag.Artists = await _artistService.GetAllArtistsAsync();
            ViewBag.Tracks = await _context.Tracks.ToListAsync();
            return View(album);
        }

        public async Task<IActionResult> Edit(int id)
        {
            var album = await _albumService.GetAlbumByIdAsync(id);
            if (album == null)
            {
                return NotFound();
            }

            ViewBag.Artists = await _artistService.GetAllArtistsAsync();
            ViewBag.Tracks = await _context.Tracks.ToListAsync();
            return View(album);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Album album, IFormFile? imageFile, int[] artistIds, int[] trackIds)
        {
            if (id != album.AlbumId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    if (imageFile != null && imageFile.Length > 0)
                    {
                        var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images/albums");
                        if (!Directory.Exists(uploadsFolder))
                        {
                            Directory.CreateDirectory(uploadsFolder);
                        }

                        var fileName = $"{Guid.NewGuid()}_{Path.GetFileName(imageFile.FileName)}";
                        var filePath = Path.Combine(uploadsFolder, fileName);

                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            await imageFile.CopyToAsync(stream);
                        }

                        album.Image = $"/images/albums/{fileName}";
                    }
                    else
                    {
                        var existingAlbum = await _albumService.GetAlbumByIdAsync(id);
                        album.Image = existingAlbum.Image;
                    }

                    await _albumService.UpdateAlbumAsync(album, artistIds, trackIds);
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", $"Lỗi khi cập nhật album: {ex.Message}");
                    if (ex.InnerException != null)
                    {
                        ModelState.AddModelError("", $"Chi tiết lỗi: {ex.InnerException.Message}");
                    }
                }
            }

            ViewBag.Artists = await _artistService.GetAllArtistsAsync();
            ViewBag.Tracks = await _context.Tracks.ToListAsync();
            return View(album);
        }

        public async Task<IActionResult> Delete(int id)
        {
            var album = await _albumService.GetAlbumByIdAsync(id);
            if (album == null)
            {
                return NotFound();
            }

            return View(album);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                await _albumService.DeleteAlbumAsync(id);
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Lỗi khi xóa album: {ex.Message}");
                var album = await _albumService.GetAlbumByIdAsync(id);
                return View(album);
            }
        }

        [HttpGet]
        public async Task<IActionResult> SyncFromZingMp3(string albumEncodeId = null)
        {
            try
            {
                IEnumerable<Album> albums;
                if (!string.IsNullOrEmpty(albumEncodeId))
                {
                    ViewData["AlbumEncodeId"] = albumEncodeId;
                    albums = await _albumService.SyncAlbumFromZingMp3Async(albumEncodeId);
                    TempData["Success"] = $"Đã đồng bộ album với encodeId {albumEncodeId}. Tổng số bài hát: {albums.FirstOrDefault()?.Tracks?.Count ?? 0}.";
                }
                else
                {
                    albums = await _albumService.GetAllAlbumsAsync();
                }
                return View("SyncAlbumFromZingMp3", albums);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] Error syncing album from Zing MP3: {ex.Message}");
                TempData["Error"] = $"Lỗi khi đồng bộ album từ Zing MP3: {ex.Message}";
                return View("SyncAlbumFromZingMp3", await _albumService.GetAllAlbumsAsync());
            }
        }
    }
}
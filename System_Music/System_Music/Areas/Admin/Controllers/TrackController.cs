using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System_Music.Models.SqlModels;
using System_Music.Services.Interfaces;

namespace System_Music.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class TrackController : Controller
    {
        private readonly ITrackService _trackService;
        private readonly IArtistService _artistService;
        private readonly IAlbumService _albumService;
        private readonly IGenreService _genreService;

        public TrackController(ITrackService trackService, IArtistService artistService, IAlbumService albumService, IGenreService genreService)
        {
            _trackService = trackService;
            _artistService = artistService;
            _albumService = albumService;
            _genreService = genreService;
        }

        public async Task<IActionResult> Index()
        {
            var tracks = await _trackService.GetAllTracksAsync();
            return View(tracks);
        }

        public async Task<IActionResult> Details(int id)
        {
            var track = await _trackService.GetTrackByIdAsync(id);
            if (track == null)
            {
                return NotFound();
            }
            return View(track);
        }

        public async Task<IActionResult> Create()
        {
            ViewBag.Artists = await _artistService.GetAllArtistsAsync();
            ViewBag.Albums = await _albumService.GetAllAlbumsAsync();
            ViewBag.Genres = await _genreService.GetAllGenresAsync();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Track track, IFormFile? audioFile, IFormFile? imageFile, int[] artistIds, int[] genreIds)
        {
            Console.WriteLine($"Received Track: Title={track.Title}, Duration={track.Duration}, AlbumId={track.AlbumId}, AudioFile={(audioFile != null ? audioFile.FileName : "null")}, ImageFile={(imageFile != null ? imageFile.FileName : "null")}");
            ModelState.Remove("AudioUrl");
            ModelState.Remove("ImageUrl");
            if (audioFile == null || audioFile.Length == 0)
            {
                ModelState.AddModelError("AudioUrl", "Vui lòng chọn file nhạc.");
            }
            else
            {
                var validAudioExtensions = new[] { ".mp3", ".wav" };
                var audioExtension = Path.GetExtension(audioFile.FileName).ToLower();
                if (!validAudioExtensions.Contains(audioExtension))
                {
                    ModelState.AddModelError("AudioFile", "File âm thanh phải có định dạng .mp3 hoặc .wav.");
                }
            }

            if (imageFile != null && imageFile.Length > 0)
            {
                var validImageExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
                var imageExtension = Path.GetExtension(imageFile.FileName).ToLower();
                if (!validImageExtensions.Contains(imageExtension))
                {
                    ModelState.AddModelError("ImageFile", "File ảnh phải có định dạng .jpg, .jpeg, .png hoặc .gif.");
                }
            }

            if (!ModelState.IsValid)
            {
                Console.WriteLine("ModelState is invalid:");
                foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
                {
                    Console.WriteLine($"Validation Error: {error.ErrorMessage}");
                }
            }

            if (ModelState.IsValid)
            {
                try
                {
                    if (audioFile != null && audioFile.Length > 0)
                    {
                        var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/tracks");
                        if (!Directory.Exists(uploadsFolder))
                        {
                            Directory.CreateDirectory(uploadsFolder);
                        }

                        var audioFileName = $"{Guid.NewGuid()}_{Path.GetFileName(audioFile.FileName)}";
                        var audioFilePath = Path.Combine(uploadsFolder, audioFileName);

                        using (var stream = new FileStream(audioFilePath, FileMode.Create))
                        {
                            await audioFile.CopyToAsync(stream);
                        }

                        track.AudioUrl = $"/tracks/{audioFileName}";
                    }

                    if (imageFile != null && imageFile.Length > 0)
                    {
                        var imagesFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images/tracks");
                        if (!Directory.Exists(imagesFolder))
                        {
                            Directory.CreateDirectory(imagesFolder);
                        }

                        var imageExtension = Path.GetExtension(imageFile.FileName);
                        var imageFileName = $"{Guid.NewGuid()}{imageExtension}";
                        var imageFilePath = Path.Combine(imagesFolder, imageFileName);

                        using (var stream = new FileStream(imageFilePath, FileMode.Create))
                        {
                            await imageFile.CopyToAsync(stream);
                        }

                        track.ImageUrl = $"/images/tracks/{imageFileName}";
                    }
                    else if (string.IsNullOrEmpty(track.ImageUrl))
                    {
                        track.ImageUrl = "/images/default-track-image.jpg";
                    }

                    track.CreatedDate = DateTime.UtcNow;
                    track.UpdatedDate = DateTime.UtcNow;

                    Console.WriteLine("Calling AddTrackAsync...");
                    await _trackService.AddTrackAsync(track, artistIds, genreIds);
                    Console.WriteLine("Track saved successfully.");
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", $"Lỗi khi lưu bài hát: {ex.Message}");
                    Console.WriteLine($"Exception in Create: {ex.Message}");
                }
            }

            ViewBag.Artists = await _artistService.GetAllArtistsAsync();
            ViewBag.Albums = await _albumService.GetAllAlbumsAsync();
            ViewBag.Genres = await _genreService.GetAllGenresAsync();
            return View(track);
        }

        public async Task<IActionResult> Edit(int id)
        {
            var track = await _trackService.GetTrackByIdAsync(id);
            if (track == null)
            {
                return NotFound();
            }

            ViewBag.Artists = await _artistService.GetAllArtistsAsync();
            ViewBag.Albums = await _albumService.GetAllAlbumsAsync();
            ViewBag.Genres = await _genreService.GetAllGenresAsync();

            return View(track);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Track track, IFormFile? audioFile, IFormFile? imageFile, int[] artistIds, int[] genreIds)
        {
            if (id != track.TrackId)
            {
                return NotFound();
            }

            Console.WriteLine($"Received Track for Edit: Title={track.Title}, Duration={track.Duration}, AlbumId={track.AlbumId}, AudioFile={(audioFile != null ? audioFile.FileName : "null")}, ImageFile={(imageFile != null ? imageFile.FileName : "null")}");

            ModelState.Remove("AudioUrl");
            ModelState.Remove("ImageUrl");

            if (audioFile != null && audioFile.Length > 0)
            {
                var validAudioExtensions = new[] { ".mp3", ".wav" };
                var audioExtension = Path.GetExtension(audioFile.FileName).ToLower();
                if (!validAudioExtensions.Contains(audioExtension))
                {
                    ModelState.AddModelError("AudioFile", "File âm thanh phải có định dạng .mp3 hoặc .wav.");
                }
            }

            if (imageFile != null && imageFile.Length > 0)
            {
                var validImageExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
                var imageExtension = Path.GetExtension(imageFile.FileName).ToLower();
                if (!validImageExtensions.Contains(imageExtension))
                {
                    ModelState.AddModelError("ImageFile", "File ảnh phải có định dạng .jpg, .jpeg, .png hoặc .gif.");
                }
            }

            if (!ModelState.IsValid)
            {
                Console.WriteLine("ModelState is invalid:");
                foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
                {
                    Console.WriteLine($"Validation Error: {error.ErrorMessage}");
                }
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var existingTrack = await _trackService.GetTrackByIdAsync(track.TrackId);
                    if (existingTrack == null)
                    {
                        return NotFound();
                    }

                    if (audioFile != null && audioFile.Length > 0)
                    {
                        var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/tracks");
                        if (!Directory.Exists(uploadsFolder))
                        {
                            Directory.CreateDirectory(uploadsFolder);
                        }

                        var audioFileName = $"{Guid.NewGuid()}_{Path.GetFileName(audioFile.FileName)}";
                        var audioFilePath = Path.Combine(uploadsFolder, audioFileName);

                        using (var stream = new FileStream(audioFilePath, FileMode.Create))
                        {
                            await audioFile.CopyToAsync(stream);
                        }

                        track.AudioUrl = $"/tracks/{audioFileName}";
                    }
                    else
                    {
                        track.AudioUrl = existingTrack.AudioUrl;
                    }

                    if (imageFile != null && imageFile.Length > 0)
                    {
                        var imagesFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images/tracks");
                        if (!Directory.Exists(imagesFolder))
                        {
                            Directory.CreateDirectory(imagesFolder);
                        }

                        var imageExtension = Path.GetExtension(imageFile.FileName);
                        var imageFileName = $"{Guid.NewGuid()}{imageExtension}";
                        var imageFilePath = Path.Combine(imagesFolder, imageFileName);

                        using (var stream = new FileStream(imageFilePath, FileMode.Create))
                        {
                            await imageFile.CopyToAsync(stream);
                        }

                        track.ImageUrl = $"/images/tracks/{imageFileName}";
                    }
                    else
                    {
                        track.ImageUrl = existingTrack.ImageUrl;
                    }

                    await _trackService.UpdateTrackAsync(track, artistIds, genreIds);
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", $"Lỗi khi cập nhật bài hát: {ex.Message}");
                    Console.WriteLine($"Exception in Edit: {ex.Message}");
                }
            }

            ViewBag.Artists = await _artistService.GetAllArtistsAsync();
            ViewBag.Albums = await _albumService.GetAllAlbumsAsync();
            ViewBag.Genres = await _genreService.GetAllGenresAsync();
            return View(track);
        }

        public async Task<IActionResult> Delete(int id)
        {
            var track = await _trackService.GetTrackByIdAsync(id);
            if (track == null)
            {
                return NotFound();
            }
            return View(track);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                await _trackService.DeleteTrackAsync(id);
                TempData["Success"] = "Xóa bài hát thành công.";
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Lỗi khi xóa bài hát: {ex.Message}";
            }
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> SyncFromZingMp3(string encodeId = null)
        {
            try
            {
                IEnumerable<Track> tracks;
                if (!string.IsNullOrEmpty(encodeId))
                {
                    ViewData["EncodeId"] = encodeId;
                    tracks = await _trackService.SyncTracksFromZingMp3Async(encodeId);
                    TempData["Success"] = $"Đã đồng bộ bài hát với encodeId {encodeId}.";
                }
                else
                {
                    tracks = await _trackService.GetAllTracksAsync();
                }
                return View(tracks);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] Error syncing track from Zing MP3: {ex.Message}");
                TempData["Error"] = $"Lỗi khi đồng bộ bài hát từ Zing MP3: {ex.Message}";
                return View(await _trackService.GetAllTracksAsync());
            }
        }
        [HttpGet]
        public async Task<IActionResult> SyncArtistSongsFromZingMp3(string artistId = null)
        {
            try
            {
                IEnumerable<Track> tracks;
                if (!string.IsNullOrEmpty(artistId))
                {
                    tracks = await _trackService.SyncArtistSongsFromZingMp3Async(artistId);
                    TempData["Success"] = $"Đã đồng bộ bài hát của nghệ sĩ với artistId {artistId}. Tổng số bài hát: {tracks.Count()}.";
                }
                else
                {
                    tracks = await _trackService.GetAllTracksAsync();
                }
                return View("SyncFromZingMp3", tracks);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] Error syncing artist songs from Zing MP3: {ex.Message}");
                TempData["Error"] = $"Lỗi khi đồng bộ bài hát của nghệ sĩ từ Zing MP3: {ex.Message}";
                return View("SyncFromZingMp3", await _trackService.GetAllTracksAsync());
            }
        }
    }
}
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System_Music.Models.SqlModels;
using System_Music.Models.DTOs;
using System_Music.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using System.Linq;

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
        private readonly IMediaService _mediaService;

        public TrackController(
            ITrackService trackService, 
            IArtistService artistService, 
            IAlbumService albumService, 
            IGenreService genreService,
            IMediaService mediaService)
        {
            _trackService = trackService;
            _artistService = artistService;
            _albumService = albumService;
            _genreService = genreService;
            _mediaService = mediaService;
        }

        public async Task<IActionResult> Index()
        {
            var tracks = await _trackService.GetAllTracksAsync();
            return View(tracks);
        }

        public async Task<IActionResult> Details(int id)
        {
            var trackDto = await _trackService.GetTrackWithDetailsAsync(id);
            if (trackDto == null) return NotFound();
            return View(trackDto);
        }

        public async Task<IActionResult> Create()
        {
            ViewBag.Artists = await _artistService.GetAllArtistsAsync();
            ViewBag.Albums = await _albumService.GetAllAlbumsAsync();
            ViewBag.Genres = await _genreService.GetAllGenresAsync();
            return View(new TrackDto());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(TrackDto trackDto, IFormFile? audioFile, IFormFile? imageFile, int[] artistIds, int[] genreIds)
        {
            if (audioFile == null || audioFile.Length == 0)
                ModelState.AddModelError("AudioUrl", "Vui lòng chọn file nhạc.");

            if (ModelState.IsValid)
            {
                try
                {
                    if (audioFile != null && audioFile.Length > 0)
                        trackDto.AudioUrl = await _mediaService.UploadFileAsync(audioFile, "tracks");

                    if (imageFile != null && imageFile.Length > 0)
                        trackDto.ImageUrl = await _mediaService.UploadFileAsync(imageFile, "images/tracks");
                    else if (string.IsNullOrEmpty(trackDto.ImageUrl))
                        trackDto.ImageUrl = "/images/default-track-image.jpg";

                    await _trackService.AddTrackAsync(trackDto, artistIds, genreIds);
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", $"Lỗi khi lưu bài hát: {ex.Message}");
                }
            }

            ViewBag.Artists = await _artistService.GetAllArtistsAsync();
            ViewBag.Albums = await _albumService.GetAllAlbumsAsync();
            ViewBag.Genres = await _genreService.GetAllGenresAsync();
            return View(trackDto);
        }

        public async Task<IActionResult> Edit(int id)
        {
            var trackDto = await _trackService.GetTrackWithDetailsAsync(id);
            if (trackDto == null) return NotFound();

            ViewBag.Artists = await _artistService.GetAllArtistsAsync();
            ViewBag.Albums = await _albumService.GetAllAlbumsAsync();
            ViewBag.Genres = await _genreService.GetAllGenresAsync();

            return View(trackDto);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, TrackDto trackDto, IFormFile? audioFile, IFormFile? imageFile, int[] artistIds, int[] genreIds)
        {
            if (id != trackDto.TrackId) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    var existingTrack = await _trackService.GetTrackByIdAsync(trackDto.TrackId);
                    if (existingTrack == null) return NotFound();

                    if (audioFile != null && audioFile.Length > 0)
                        trackDto.AudioUrl = await _mediaService.UploadFileAsync(audioFile, "tracks");
                    else
                        trackDto.AudioUrl = existingTrack.AudioUrl;

                    if (imageFile != null && imageFile.Length > 0)
                        trackDto.ImageUrl = await _mediaService.UploadFileAsync(imageFile, "images/tracks");
                    else
                        trackDto.ImageUrl = existingTrack.ImageUrl;

                    await _trackService.UpdateTrackAsync(trackDto, artistIds, genreIds);
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", $"Lỗi khi cập nhật bài hát: {ex.Message}");
                }
            }

            ViewBag.Artists = await _artistService.GetAllArtistsAsync();
            ViewBag.Albums = await _albumService.GetAllAlbumsAsync();
            ViewBag.Genres = await _genreService.GetAllGenresAsync();
            return View(trackDto);
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
                IEnumerable<TrackDto> tracks;
                if (!string.IsNullOrEmpty(encodeId))
                {
                    ViewData["EncodeId"] = encodeId;
                    await _trackService.SyncTracksFromZingMp3Async(encodeId);
                    TempData["Success"] = $"Đã đồng bộ bài hát với encodeId {encodeId}.";
                }
                
                tracks = await _trackService.GetAllTracksAsync();
                return View(tracks);
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Lỗi khi đồng bộ bài hát từ Zing MP3: {ex.Message}";
                return View(await _trackService.GetAllTracksAsync());
            }
        }
    }
}
using Microsoft.AspNetCore.Mvc;
using System_Music.Models.SqlModels;
using System_Music.Models.DTOs;
using System_Music.Services.Interfaces;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using Microsoft.AspNetCore.Http;
using System.Linq;

namespace System_Music.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class AlbumController : Controller
    {
        private readonly IAlbumService _albumService;
        private readonly IArtistService _artistService;
        private readonly ITrackService _trackService;
        private readonly IMediaService _mediaService;

        public AlbumController(
            IAlbumService albumService, 
            IArtistService artistService, 
            ITrackService trackService,
            IMediaService mediaService)
        {
            _albumService = albumService;
            _artistService = artistService;
            _trackService = trackService;
            _mediaService = mediaService;
        }

        public async Task<IActionResult> Index()
        {
            var albums = await _albumService.GetAllAlbumsAsync();
            return View(albums);
        }

        public async Task<IActionResult> Details(int id)
        {
            var albumDto = await _albumService.GetAlbumByIdWithDetailsAsync(id);
            if (albumDto == null) return NotFound();
            return View(albumDto);
        }

        public async Task<IActionResult> Create()
        {
            ViewBag.Artists = await _artistService.GetAllArtistsAsync();
            ViewBag.Tracks = await _trackService.GetAllTracksAsync();
            return View(new AlbumDto());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(AlbumDto albumDto, IFormFile? imageFile, int[] artistIds, int[] trackIds)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    if (imageFile != null && imageFile.Length > 0)
                    {
                        albumDto.ImageUrl = await _mediaService.UploadFileAsync(imageFile, "images/albums");
                    }

                    await _albumService.AddAlbumAsync(albumDto, artistIds, trackIds);
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", $"Lỗi khi tạo album: {ex.Message}");
                }
            }

            ViewBag.Artists = await _artistService.GetAllArtistsAsync();
            ViewBag.Tracks = await _trackService.GetAllTracksAsync();
            return View(albumDto);
        }

        public async Task<IActionResult> Edit(int id)
        {
            var albumDto = await _albumService.GetAlbumByIdWithDetailsAsync(id);
            if (albumDto == null) return NotFound();

            ViewBag.Artists = await _artistService.GetAllArtistsAsync();
            ViewBag.Tracks = await _trackService.GetAllTracksAsync();
            return View(albumDto);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, AlbumDto albumDto, IFormFile? imageFile, int[] artistIds, int[] trackIds)
        {
            if (id != albumDto.AlbumId) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    if (imageFile != null && imageFile.Length > 0)
                    {
                        albumDto.ImageUrl = await _mediaService.UploadFileAsync(imageFile, "images/albums");
                    }
                    else
                    {
                        var existingAlbum = await _albumService.GetAlbumByIdAsync(id);
                        albumDto.ImageUrl = existingAlbum.ImageUrl;
                    }

                    await _albumService.UpdateAlbumAsync(albumDto, artistIds, trackIds);
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", $"Lỗi khi cập nhật album: {ex.Message}");
                }
            }

            ViewBag.Artists = await _artistService.GetAllArtistsAsync();
            ViewBag.Tracks = await _trackService.GetAllTracksAsync();
            return View(albumDto);
        }

        public async Task<IActionResult> Delete(int id)
        {
            var albumDto = await _albumService.GetAlbumByIdAsync(id);
            if (albumDto == null) return NotFound();
            return View(albumDto);
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
                var albumDto = await _albumService.GetAlbumByIdAsync(id);
                return View(albumDto);
            }
        }

        [HttpGet]
        public async Task<IActionResult> SyncFromZingMp3(string albumEncodeId = null)
        {
            try
            {
                IEnumerable<AlbumDto> albums;
                if (!string.IsNullOrEmpty(albumEncodeId))
                {
                    ViewData["AlbumEncodeId"] = albumEncodeId;
                    albums = await _albumService.SyncAlbumFromZingMp3Async(albumEncodeId);
                    TempData["Success"] = $"Đã đồng bộ album với encodeId {albumEncodeId}.";
                }
                else
                {
                    albums = await _albumService.GetAllAlbumsAsync();
                }
                return View("SyncAlbumFromZingMp3", albums);
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Lỗi khi đồng bộ album từ Zing MP3: {ex.Message}";
                return View("SyncAlbumFromZingMp3", await _albumService.GetAllAlbumsAsync());
            }
        }
    }
}
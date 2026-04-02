using Microsoft.AspNetCore.Authorization;
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
    [Authorize(Roles = "Admin")]
    public class ArtistController : Controller
    {
        private readonly IArtistService _artistService;
        private readonly IMediaService _mediaService;

        public ArtistController(IArtistService artistService, IMediaService mediaService)
        {
            _artistService = artistService;
            _mediaService = mediaService;
        }

        public async Task<IActionResult> Index()
        {
            var artists = await _artistService.GetAllArtistsAsync();
            return View(artists);
        }

        public async Task<IActionResult> Details(int id)
        {
            var artistDto = await _artistService.GetArtistByIdAsync(id);
            if (artistDto == null) return NotFound();
            return View(artistDto);
        }

        public IActionResult Create()
        {
            return View(new ArtistDto());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ArtistDto artistDto, IFormFile? ImageFile, string ImageUrl)
        {
            if (!string.IsNullOrWhiteSpace(ImageUrl))
            {
                artistDto.ImageUrl = ImageUrl;
            }
            else if (ImageFile != null && ImageFile.Length > 0)
            {
                artistDto.ImageUrl = await _mediaService.UploadFileAsync(ImageFile, "images/artists");
            }

            if (ModelState.IsValid)
            {
                await _artistService.AddArtistAsync(artistDto);
                return RedirectToAction(nameof(Index));
            }

            return View(artistDto);
        }

        public async Task<IActionResult> Edit(int id)
        {
            var artistDto = await _artistService.GetArtistByIdAsync(id);
            if (artistDto == null) return NotFound();
            return View(artistDto);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, ArtistDto artistDto, IFormFile? ImageFile)
        {
            if (id != artistDto.ArtistId) return NotFound();

            if (ImageFile != null && ImageFile.Length > 0)
            {
                artistDto.ImageUrl = await _mediaService.UploadFileAsync(ImageFile, "images/artists");
            }

            if (ModelState.IsValid)
            {
                await _artistService.UpdateArtistAsync(artistDto);
                return RedirectToAction(nameof(Index));
            }
            return View(artistDto);
        }

        public async Task<IActionResult> Delete(int id)
        {
            var artistDto = await _artistService.GetArtistByIdAsync(id);
            if (artistDto == null) return NotFound();
            return View(artistDto);
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
                IEnumerable<ArtistDto> artists;
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
                TempData["Error"] = "Failed to sync artists from Zing MP3. Please try again later.";
                return RedirectToAction(nameof(Index));
            }
        }
    }
}
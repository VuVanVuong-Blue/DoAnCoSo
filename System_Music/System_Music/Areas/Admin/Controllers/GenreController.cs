using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System_Music.Models.SqlModels;
using System_Music.Models.DTOs;
using System_Music.Services.Interfaces;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;

namespace System_Music.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
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
            return View(new GenreDto());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(GenreDto genreDto)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    await _genreService.AddGenreAsync(genreDto);
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError(string.Empty, ex.Message);
                }
            }
            return View(genreDto);
        }

        public async Task<IActionResult> Edit(int id)
        {
            try
            {
                var genreDto = await _genreService.GetGenreByIdAsync(id);
                return View(genreDto); 
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, GenreDto genreDto)
        {
            if (id != genreDto.GenreId) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    await _genreService.UpdateGenreAsync(genreDto);
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError(string.Empty, ex.Message);
                }
            }
            return View(genreDto);
        }

        public async Task<IActionResult> Details(int id)
        {
            try
            {
                var genreDto = await _genreService.GetGenreWithDetailsAsync(id);
                return View(genreDto);
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
        }

        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var genreDto = await _genreService.GetGenreByIdAsync(id);
                return View(genreDto);
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
                var genreDto = await _genreService.GetGenreByIdAsync(id);
                return View(genreDto);
            }
        }

        [HttpGet]
        public async Task<IActionResult> SyncFromZingMp3(bool refresh = false)
        {
            try
            {
                IEnumerable<GenreDto> genres;
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
                TempData["Error"] = $"Failed to sync genres: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }
    }
}
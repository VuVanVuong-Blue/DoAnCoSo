using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System_Music.Models.SqlModels;
using System_Music.Models.ViewModels;
using System_Music.Services.Implementations;
using System_Music.Services.Interfaces;
using System.Threading.Tasks;

namespace System_Music.Controllers
{
    public class ArtistController : Controller
    {
        private readonly IArtistService _artistService;
        private readonly IAlbumService _albumService;
        private readonly SmartMusicDbContext _context;

        public ArtistController(IArtistService artistService, IAlbumService albumService, SmartMusicDbContext context)
        {
            _artistService = artistService;
            _albumService = albumService;
            _context = context;
        }

        [HttpGet]
        [Route("Artist/Detail/{id}")]
        public async Task<IActionResult> Detail(int id)
        {
            var artist = await _artistService.GetArtistByIdAsync(id);
            if (artist == null)
            {
                return NotFound();
            }

            var tracks = await _artistService.GetTracksByArtistIdAsync(id);
            var albums = await _albumService.GetAlbumsByArtistAsync(id);

            var model = new ArtistProfileViewModel
            {
                Artist = artist,
                Tracks = tracks,
                Albums = albums
            };

            return View("~/Views/Home/ArtistProfile.cshtml", model);
        }

        [HttpGet]
        [Route("Artist/{id}")]
        public async Task<IActionResult> Index(int id)
        {
            var artist = await _artistService.GetArtistByIdAsync(id);
            if (artist == null)
            {
                return NotFound();
            }

            var tracks = await _artistService.GetTracksByArtistIdAsync(id);
            var albums = await _albumService.GetAlbumsByArtistAsync(id);

            var model = new ArtistProfileViewModel
            {
                Artist = artist,
                Tracks = tracks,
                Albums = albums
            };

            return View("~/Views/Home/ArtistProfile.cshtml", model);
        }
    }
}
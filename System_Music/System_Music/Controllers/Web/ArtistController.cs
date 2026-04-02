using Microsoft.AspNetCore.Mvc;
using System_Music.Models.ViewModels;
using System_Music.Services.Interfaces;
using System.Threading.Tasks;

namespace System_Music.Controllers.Web
{
    public class ArtistController : Controller
    {
        private readonly IArtistService _artistService;
        private readonly IAlbumService _albumService;

        public ArtistController(IArtistService artistService, IAlbumService albumService)
        {
            _artistService = artistService;
            _albumService = albumService;
        }

        [HttpGet]
        [Route("Artist/Detail/{id}")]
        public async Task<IActionResult> Detail(int id)
        {
            var artistDto = await _artistService.GetArtistByIdAsync(id);
            if (artistDto == null)
            {
                return NotFound();
            }

            var tracksDto = await _artistService.GetTracksByArtistIdAsync(id);
            var albumsDto = await _albumService.GetAlbumsByArtistAsync(id);

            var model = new ArtistProfileViewModel
            {
                Artist = artistDto,
                Tracks = tracksDto,
                Albums = albumsDto
            };

            return View("~/Views/Home/ArtistProfile.cshtml", model);
        }

        [HttpGet]
        [Route("Artist/{id}")]
        public async Task<IActionResult> Index(int id)
        {
            return await Detail(id);
        }
    }
}
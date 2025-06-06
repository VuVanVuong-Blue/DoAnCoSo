using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System_Music.Models.SqlModels;
using System_Music.Models.ViewModels;
using System.Threading.Tasks;

namespace System_Music.Controllers
{
    public class AlbumController : Controller
    {
        private readonly SmartMusicDbContext _context;

        public AlbumController(SmartMusicDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        [Route("Album/Detail/{id}")]
        public IActionResult Detail(int id)
        {
            return RedirectToAction("DetailAlbum", "Home", new { id = id });
        }
    }
}
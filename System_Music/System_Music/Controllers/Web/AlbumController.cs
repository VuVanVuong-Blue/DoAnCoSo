using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace System_Music.Controllers.Web
{
    public class AlbumController : Controller
    {
        public AlbumController()
        {
        }

        [HttpGet]
        [Route("Album/Detail/{id}")]
        public IActionResult Detail(int id)
        {
            return RedirectToAction("DetailAlbum", "Home", new { id = id });
        }
    }
}
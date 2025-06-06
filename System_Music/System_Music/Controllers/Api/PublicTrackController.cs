using Microsoft.AspNetCore.Mvc;
using System_Music.Services.Interfaces;

namespace System_Music.Controllers
{
    [Route("api/public/tracks")]
    [ApiController]
    public class PublicTrackController : ControllerBase
    {
        private readonly ITrackService _trackService;

        public PublicTrackController(ITrackService trackService)
        {
            _trackService = trackService;
        }

        [HttpGet("by-album/{albumId}")]
        public async Task<IActionResult> GetTracksByAlbum(int albumId)
        {
            var tracks = await _trackService.GetTracksByAlbumAsync(albumId);
            if (tracks == null || !tracks.Any())
            {
                return NotFound("Không tìm thấy bài hát nào trong album này.");
            }

            var trackDtos = tracks.Select(t => new
            {
                title = t.Title,
                duration = t.Duration,
                audioUrl = t.AudioUrl,
                imageUrl = t.ImageUrl, // Thêm ImageUrl
                trackArtists = t.TrackArtists.Select(ta => new
                {
                    artist = new { name = ta.Artist.Name }
                }).ToList()
            }).ToList();

            return Ok(trackDtos);
        }
    }
}
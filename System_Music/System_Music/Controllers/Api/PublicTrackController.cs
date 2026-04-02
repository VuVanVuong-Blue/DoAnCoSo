using Microsoft.AspNetCore.Mvc;
using System_Music.Models.Common;
using System_Music.Models.DTOs;
using System_Music.Services.Interfaces;

namespace System_Music.Controllers.Api
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
                return NotFound(ApiResult<object>.Failure("Không tìm thấy bài hát nào trong album này."));
            }

            return Ok(ApiResult<List<TrackDto>>.Success(tracks));
        }

        [HttpGet("top/{count}")]
        public async Task<IActionResult> GetTopTracks(int count = 10)
        {
            var tracks = await _trackService.GetTopTracksAsync(count);
            return Ok(ApiResult<List<TrackDto>>.Success(tracks));
        }
    }
}
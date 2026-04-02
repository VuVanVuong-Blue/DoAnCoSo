using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System_Music.Models.DTOs;
using System_Music.Services.Interfaces;
using System.Security.Claims;
using System_Music.Models.Common;
using System_Music.Models.SqlModels;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;

namespace System_Music.Controllers.Api
{
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme + "," + IdentityConstants.ApplicationScheme)]
    [Route("api/[controller]")]
    [ApiController]
    public class TrackController : ControllerBase
    {
        private readonly ITrackService _trackService;
        private readonly IListenHistoryService _listenHistoryService;

        public TrackController(ITrackService trackService, IListenHistoryService listenHistoryService)
        {
            _trackService = trackService;
            _listenHistoryService = listenHistoryService;
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetTrack(int id)
        {
            var track = await _trackService.GetTrackByIdAsync(id);
            if (track == null) return NotFound(ApiResult<object>.Failure("Không tìm thấy bài hát"));
            return Ok(ApiResult<TrackDto>.Success(track));
        }

        [HttpGet("Current")]
        public async Task<IActionResult> GetCurrentTrack(int? trackId = null)
        {
            // Simple logic: if trackId provided, get it, else get top track or first track
            TrackDto track;
            if (trackId.HasValue)
            {
                track = await _trackService.GetTrackByIdAsync(trackId.Value);
            }
            else
            {
                var tracks = await _trackService.GetAllTracksAsync();
                track = tracks.FirstOrDefault();
            }

            if (track == null) return NotFound(ApiResult<object>.Failure("Không có bài hát nào"));
            return Ok(ApiResult<TrackDto>.Success(track));
        }

        [HttpPost("AddListenHistory")]
        public async Task<IActionResult> AddListenHistory([FromBody] AddListenHistoryRequest request)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var track = await _trackService.GetTrackByIdAsync(request.TrackId);
            if (track == null) return NotFound(ApiResult<object>.Failure("Không tìm thấy bài hát"));

            var listenHistory = new ListenHistory
            {
                UserId = userId,
                TrackId = request.TrackId,
                ListenDate = DateTime.UtcNow,
                EntityType = EntityType.Track
            };

            await _listenHistoryService.AddListenHistoryAsync(listenHistory);
            return Ok(ApiResult<object>.Success(null, "Đã lưu lịch sử nghe"));
        }
    }

    public class AddListenHistoryRequest
    {
        public int TrackId { get; set; }
    }
}

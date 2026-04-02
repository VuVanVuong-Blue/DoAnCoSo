using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System_Music.Models.SqlModels;
using System_Music.Services.Interfaces;
using System_Music.Models.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;

namespace System_Music.Controllers.Api
{
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme + "," + IdentityConstants.ApplicationScheme)]
    [Route("api/[controller]")]
    [ApiController]
    public class ListenHistoryController : ControllerBase
    {
        private readonly IListenHistoryService _listenHistoryService;
        private readonly ILogger<ListenHistoryController> _logger;

        public ListenHistoryController(IListenHistoryService listenHistoryService, ILogger<ListenHistoryController> logger)
        {
            _listenHistoryService = listenHistoryService;
            _logger = logger;
        }

        [HttpPost("Add")]
        public async Task<IActionResult> Add([FromBody] ListenHistoryRequest request)
        {
            try
            {
                if (request == null || string.IsNullOrEmpty(request.EntityType))
                {
                    return BadRequest(ApiResult<object>.Failure("Dữ liệu đầu vào không hợp lệ."));
                }

                if (!Enum.TryParse<EntityType>(request.EntityType, true, out var entityType))
                {
                    return BadRequest(ApiResult<object>.Failure("EntityType không hợp lệ."));
                }

                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(ApiResult<object>.Failure("Không thể xác định người dùng."));
                }

                var listenHistory = new ListenHistory
                {
                    UserId = userId,
                    EntityType = entityType,
                    TrackId = entityType == EntityType.Track ? request.EntityId : null,
                    AlbumId = entityType == EntityType.Album ? request.EntityId : null,
                    ArtistId = entityType == EntityType.Artist ? request.EntityId : null,
                    PlaylistId = entityType == EntityType.Playlist ? request.EntityId : null,
                    ListenDate = DateTime.UtcNow
                };

                await _listenHistoryService.AddListenHistoryAsync(listenHistory);
                return Ok(ApiResult<object>.Success(null, "Lịch sử đã được lưu."));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving listen history");
                return StatusCode(500, ApiResult<object>.Failure($"Lỗi khi lưu lịch sử: {ex.Message}"));
            }
        }
    }

    public class ListenHistoryRequest
    {
        public string EntityType { get; set; }
        public int EntityId { get; set; }
    }
}
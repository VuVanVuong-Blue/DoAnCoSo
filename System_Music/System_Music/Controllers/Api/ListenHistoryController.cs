using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using System.Security.Claims;
using System_Music.Models.SqlModels;
using System_Music.Services.Interfaces;
using Microsoft.Extensions.Logging; // Thêm namespace này

[Route("api/[controller]")]
[ApiController]
public class ListenHistoryController : ControllerBase
{
    private readonly IListenHistoryService _listenHistoryService;
    private readonly ILogger<ListenHistoryController> _logger; // Thêm logger

    public ListenHistoryController(IListenHistoryService listenHistoryService, ILogger<ListenHistoryController> logger)
    {
        _listenHistoryService = listenHistoryService;
        _logger = logger;
    }

    [HttpPost("Add")]
    public async Task<IActionResult> Add([FromBody] ListenHistoryRequest request)
    {
        _logger.LogInformation("Received request to add listen history: {@Request}", request);

        try
        {
            if (request == null || string.IsNullOrEmpty(request.EntityType) || string.IsNullOrEmpty(request.EntityId))
            {
                _logger.LogWarning("Invalid request data: {@Request}", request);
                return BadRequest(new { success = false, message = "Dữ liệu đầu vào không hợp lệ." });
            }

            if (!Enum.TryParse<EntityType>(request.EntityType, true, out var entityType))
            {
                _logger.LogWarning("Invalid EntityType: {@EntityType}", request.EntityType);
                return BadRequest(new { success = false, message = "EntityType không hợp lệ." });
            }

            if (!int.TryParse(request.EntityId, out var entityId))
            {
                _logger.LogWarning("Invalid EntityId: {@EntityId}", request.EntityId);
                return BadRequest(new { success = false, message = "EntityId không hợp lệ, phải là một số nguyên." });
            }

            var userId = HttpContext.User?.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                _logger.LogWarning("UserId not found in HttpContext");
                return Unauthorized(new { success = false, message = "Không thể xác định người dùng." });
            }

            var listenHistory = new ListenHistory
            {
                UserId = userId,
                EntityType = entityType,
                TrackId = entityType == EntityType.Track ? entityId : null,
                AlbumId = entityType == EntityType.Album ? entityId : null,
                ArtistId = entityType == EntityType.Artist ? entityId : null,
                PlaylistId = entityType == EntityType.Playlist ? entityId : null,
                ListenDate = DateTime.UtcNow
            };

            _logger.LogInformation("Saving listen history: {@ListenHistory}", listenHistory);
            await _listenHistoryService.AddListenHistoryAsync(listenHistory);
            _logger.LogInformation("Listen history saved successfully for UserId: {UserId}", userId);
            return Ok(new { success = true, message = "Lịch sử đã được lưu." });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving listen history: {Message}", ex.Message);
            return StatusCode(500, new { success = false, message = $"Lỗi khi lưu lịch sử: {ex.Message}" });
        }
    }
}

public class ListenHistoryRequest
{
    public string EntityType { get; set; }
    public string EntityId { get; set; }
}
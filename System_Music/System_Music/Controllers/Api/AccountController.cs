using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System_Music.Models.SqlModels;
using System_Music.Services.Interfaces;

namespace System_Music.Controllers.Api
{
    [Authorize]
    [Route("User")]
    public class AccountController : Controller
    {
        private readonly UserManager<User> _userManager;
        private readonly IUserMediaService _userMediaService;

        public AccountController(UserManager<User> userManager, IUserMediaService userMediaService)
        {
            _userManager = userManager;
            _userMediaService = userMediaService;
        }

        [HttpPost]
        [Route("UploadAvatar")]
        public async Task<IActionResult> UploadAvatar(IFormFile image)
        {
            try
            {
                if (image == null || image.Length == 0)
                {
                    return BadRequest("Không có file ảnh được chọn!");
                }

                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                {
                    return NotFound("Người dùng không tồn tại!");
                }

                // Lưu file
                var uploadsDir = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads");
                if (!Directory.Exists(uploadsDir))
                {
                    Directory.CreateDirectory(uploadsDir);
                }

                var fileName = $"{userId}_{Guid.NewGuid()}{Path.GetExtension(image.FileName)}";
                var filePath = Path.Combine(uploadsDir, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await image.CopyToAsync(stream);
                }

                // Xóa ảnh cũ nếu có
                if (user.AvatarMediaId.HasValue)
                {
                    await _userMediaService.DeleteUserMediaAsync(user.AvatarMediaId.Value);
                }

                // Tạo bản ghi UserMedia
                var userMedia = new UserMedia
                {
                    UserId = userId,
                    MediaPath = $"/uploads/{fileName}",
                    PlaylistId = null, // Không liên kết với playlist
                    UploadTime = DateTime.UtcNow
                };
                await _userMediaService.AddUserMediaAsync(userMedia);

                // Cập nhật User
                user.AvatarMediaId = userMedia.MediaId;
                await _userManager.UpdateAsync(user);

                return Ok(new { ImagePath = userMedia.MediaPath });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}

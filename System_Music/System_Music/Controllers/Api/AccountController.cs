using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System_Music.Models.SqlModels;
using System_Music.Services.Interfaces;
using System_Music.Models.Common;
using System_Music.Models.DTOs;
using AutoMapper;
using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace System_Music.Controllers.Api
{
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme + "," + IdentityConstants.ApplicationScheme)]
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly UserManager<User> _userManager;
        private readonly IUserMediaService _userMediaService;
        private readonly IMapper _mapper;

        public AccountController(
            UserManager<User> userManager, 
            IUserMediaService userMediaService,
            IMapper mapper)
        {
            _userManager = userManager;
            _userMediaService = userMediaService;
            _mapper = mapper;
        }

        [HttpGet("profile")]
        public async Task<IActionResult> GetProfile()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return NotFound(ApiResult<object>.Failure("Người dùng không tồn tại"));

            var userDto = _mapper.Map<UserDto>(user);
            return Ok(ApiResult<UserDto>.Success(userDto));
        }

        [HttpPost("UploadAvatar")]
        public async Task<IActionResult> UploadAvatar(IFormFile image)
        {
            if (image == null || image.Length == 0)
                return BadRequest(ApiResult<object>.Failure("Không có file ảnh được chọn!"));

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return NotFound(ApiResult<object>.Failure("Người dùng không tồn tại!"));

            // Lưu file
            var uploadsDir = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads");
            if (!Directory.Exists(uploadsDir)) Directory.CreateDirectory(uploadsDir);

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
                UploadTime = DateTime.UtcNow
            };
            await _userMediaService.AddUserMediaAsync(userMedia);

            // Cập nhật User
            user.AvatarMediaId = userMedia.MediaId;
            await _userManager.UpdateAsync(user);

            return Ok(ApiResult<object>.Success(new { ImagePath = userMedia.MediaPath }, "Cập nhật ảnh đại diện thành công"));
        }
    }
}

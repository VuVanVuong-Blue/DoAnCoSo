using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System_Music.Models.Common;
using System_Music.Models.DTOs;
using System_Music.Models.SqlModels;
using System_Music.Services.Interfaces;
using AutoMapper;

using System.Security.Claims;

namespace System_Music.Controllers.Api
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private readonly IJwtService _jwtService;
        private readonly IMapper _mapper;
        private readonly IConfiguration _configuration;

        public AuthController(
            UserManager<User> userManager,
            SignInManager<User> signInManager,
            IJwtService jwtService,
            IMapper mapper,
            IConfiguration configuration)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _jwtService = jwtService;
            _mapper = mapper;
            _configuration = configuration;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] UserRegisterRequest request)
        {
            var user = _mapper.Map<User>(request);
            var result = await _userManager.CreateAsync(user, request.Password);

            if (!result.Succeeded)
            {
                return BadRequest(ApiResult<object>.Failure("Đăng ký không thành công", 
                    result.Errors.Select(e => e.Description).ToList()));
            }

            await _userManager.AddToRoleAsync(user, "User");
            
            var userDto = _mapper.Map<UserDto>(user);
            userDto.Roles = new List<string> { "User" };

            return Ok(ApiResult<UserDto>.Success(userDto, "Đăng ký thành công"));
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] UserLoginRequest request)
        {
            var user = await _userManager.FindByEmailAsync(request.Email);
            if (user == null)
            {
                return Unauthorized(ApiResult<object>.Failure("Email hoặc mật khẩu không chính xác"));
            }

            var result = await _signInManager.CheckPasswordSignInAsync(user, request.Password, false);
            if (!result.Succeeded)
            {
                return Unauthorized(ApiResult<object>.Failure("Email hoặc mật khẩu không chính xác"));
            }

            var roles = await _userManager.GetRolesAsync(user);
            var token = _jwtService.GenerateAccessToken(user, roles);
            var refreshToken = _jwtService.GenerateRefreshToken();

            // Save Refresh Token
            var jwtSettings = _configuration.GetSection("Jwt");
            user.RefreshToken = refreshToken;
            user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(double.Parse(jwtSettings["RefreshTokenDurationInDays"] ?? "7"));
            await _userManager.UpdateAsync(user);
            
            var userDto = _mapper.Map<UserDto>(user);
            userDto.Roles = roles.ToList();

            var response = new TokenResponseDto
            {
                AccessToken = token,
                RefreshToken = refreshToken,
                Expiration = DateTime.UtcNow.AddMinutes(double.Parse(jwtSettings["DurationInMinutes"])),
                User = userDto
            };

            return Ok(ApiResult<TokenResponseDto>.Success(response, "Đăng nhập thành công"));
        }

        [HttpPost("refresh")]
        public async Task<IActionResult> Refresh([FromBody] TokenRequestDto request)
        {
            if (request == null || string.IsNullOrEmpty(request.AccessToken) || string.IsNullOrEmpty(request.RefreshToken))
            {
                return BadRequest(ApiResult<object>.Failure("Yêu cầu không hợp lệ"));
            }

            string accessToken = request.AccessToken;
            string refreshToken = request.RefreshToken;

            var principal = _jwtService.GetPrincipalFromExpiredToken(accessToken);
            var userId = principal.FindFirstValue(ClaimTypes.NameIdentifier);
            
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null || user.RefreshToken != refreshToken || user.RefreshTokenExpiryTime <= DateTime.UtcNow)
            {
                return BadRequest(ApiResult<object>.Failure("Refresh token không hợp lệ hoặc đã hết hạn"));
            }

            var roles = await _userManager.GetRolesAsync(user);
            var newAccessToken = _jwtService.GenerateAccessToken(user, roles);
            var newRefreshToken = _jwtService.GenerateRefreshToken();

            user.RefreshToken = newRefreshToken;
            await _userManager.UpdateAsync(user);

            var response = new TokenResponseDto
            {
                AccessToken = newAccessToken,
                RefreshToken = newRefreshToken,
                Expiration = DateTime.UtcNow.AddMinutes(double.Parse(_configuration["Jwt:DurationInMinutes"])),
                User = _mapper.Map<UserDto>(user)
            };

            return Ok(ApiResult<TokenResponseDto>.Success(response, "Làm mới token thành công"));
        }
    }
}

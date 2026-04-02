using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System_Music.Models.SqlModels;
using System_Music.Models.ViewModels;
using System_Music.Services.Interfaces;
using System.Security.Claims;
using System_Music.Models.Common;
using System_Music.Models.DTOs;
using AutoMapper;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;

namespace System_Music.Controllers.Api
{
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme + "," + IdentityConstants.ApplicationScheme)]
    [Route("api/[controller]")]
    [ApiController]
    public class PlaylistController : ControllerBase
    {
        private readonly IPlaylistService _playlistService;
        private readonly ISearchService _searchService;
        private readonly IMediaService _mediaService;
        private readonly IMapper _mapper;

        public PlaylistController(
            IPlaylistService playlistService,
            ISearchService searchService,
            IMediaService mediaService,
            IMapper mapper)
        {
            _playlistService = playlistService;
            _searchService = searchService;
            _mediaService = mediaService;
            _mapper = mapper;
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetPlaylist(int id)
        {
            var playlist = await _playlistService.GetPlaylistByIdAsync(id);
            if (playlist == null)
                return NotFound(ApiResult<object>.Failure("Playlist không tồn tại!"));

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!playlist.IsPublic && playlist.UserId != userId)
                return Forbid();

            return Ok(ApiResult<PlaylistDto>.Success(playlist));
        }

        [HttpPost("Create")]
        public async Task<IActionResult> Create([FromBody] PlaylistCreateModel request)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var playlistCount = (await _playlistService.GetUserPlaylistsAsync(userId)).Count;
            
            var playlist = new Playlist
            {
                Name = string.IsNullOrEmpty(request.Name) ? $"Danh sách phát của tôi #{playlistCount + 1}" : request.Name,
                UserId = userId,
                CreatedDate = DateTime.UtcNow,
                IsPublic = request.IsPublic
            };
            
            await _playlistService.CreatePlaylistAsync(playlist);
            return Ok(ApiResult<PlaylistDto>.Success(_mapper.Map<PlaylistDto>(playlist), "Tạo playlist thành công"));
        }

        [HttpGet("Search")]
        public async Task<IActionResult> Search(string searchTerm)
        {
            var results = await _searchService.SearchAllAsync(searchTerm);
            return Ok(ApiResult<SearchResultDto>.Success(results));
        }

        [HttpGet("ProxyImage")]
        public async Task<IActionResult> ProxyImage(string url)
        {
            if (string.IsNullOrEmpty(url)) return NotFound();
            
            var imageBytes = await _mediaService.GetImageBytesAsync(url);
            if (imageBytes == null) return NotFound();
            
            return File(imageBytes, "image/jpeg");
        }
        
        [HttpPost("UploadImage/{playlistId}")]
        public async Task<IActionResult> UploadImage(int playlistId, IFormFile image)
        {
            var playlist = await _playlistService.GetPlaylistByIdAsync(playlistId);
            if (playlist == null) return NotFound(ApiResult<object>.Failure("Playlist không tồn tại"));

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (playlist.UserId != userId) return Forbid();

            var imageUrl = await _mediaService.UploadFileAsync(image, "uploads");
            
            // Logic to update playlist ImageMediaId would go here or in Service
            // For now, returning the path as proof of concept.
            return Ok(ApiResult<object>.Success(new { ImagePath = imageUrl }));
        }
    }
}
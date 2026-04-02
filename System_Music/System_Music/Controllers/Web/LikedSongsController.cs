using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System_Music.Models.SqlModels;
using System_Music.Models.ViewModels;
using System_Music.Models.DTOs;
using System_Music.Services.Interfaces;
using System.Threading.Tasks;
using System.Collections.Generic;
using System;

namespace System_Music.Controllers.Web
{
    public class LikedSongsController : Controller
    {
        private readonly UserManager<User> _userManager;
        private readonly ITrackService _trackService;
        private readonly ILikeTrackService _likeTrackService;
        private readonly IMapper _mapper;

        public LikedSongsController(
            UserManager<User> userManager, 
            ITrackService trackService, 
            ILikeTrackService likeTrackService,
            IMapper mapper)
        {
            _userManager = userManager;
            _trackService = trackService;
            _likeTrackService = likeTrackService;
            _mapper = mapper;
        }

        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login", "Account", new { area = "Identity" });
            }

            var likedTracks = await _trackService.GetLikedTracksAsync(user.Id);

            var likedSongsPlaylist = new PlaylistDto
            {
                PlaylistId = 0,
                Name = "Bài hát đã thích",
                UserId = user.Id,
                IsPublic = false,
                CreatedDate = DateTime.Now,
                ImageUrl = "/images/liked-songs-placeholder.png",
                Tracks = likedTracks
            };

            var likeDates = new Dictionary<int, DateTime>();
            foreach (var track in likedTracks)
            {
                var likeTrackDto = await _likeTrackService.GetLikeByUserAndTrackAsync(user.Id, track.TrackId);
                likeDates[track.TrackId] = lookUpLikeDate(likeTrackDto);
            }

            var model = new PlaylistViewModel
            {
                Playlist = likedSongsPlaylist,
                Tracks = likedTracks,
                LikeDates = likeDates,
                CurrentUser = _mapper.Map<UserDto>(user)
            };

            return View("~/Views/Playlist/IndexPlaylist.cshtml", model);
        }

        private DateTime lookUpLikeDate(LikeTrackDto? likeTrackDto)
        {
            return likeTrackDto?.LikeDate ?? DateTime.UtcNow;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddToLikedSongs(int trackId)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            var track = await _trackService.GetTrackByIdAsync(trackId);
            if (track == null) return NotFound();

            var hasLiked = await _likeTrackService.HasLikedTrackAsync(user.Id, trackId);
            if (!hasLiked)
            {
                var likeTrackDto = new LikeTrackDto
                {
                    UserId = user.Id,
                    TrackId = trackId,
                    LikeDate = DateTime.UtcNow
                };
                await _likeTrackService.AddLikeAsync(likeTrackDto);
            }

            return Json(new { success = true });
        }
    }
}
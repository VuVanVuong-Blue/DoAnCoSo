using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System_Music.Models.SqlModels;
using System_Music.Models.ViewModels;
using System_Music.Models.DTOs;
using System_Music.Services.Interfaces;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using System;

namespace System_Music.Controllers.Web
{
    public class PlaylistController : Controller
    {
        private readonly UserManager<User> _userManager;
        private readonly IPlaylistService _playlistService;
        private readonly ITrackService _trackService;
        private readonly ILikeTrackService _likeTrackService;
        private readonly IMapper _mapper;

        public PlaylistController(
            UserManager<User> userManager, 
            IPlaylistService playlistService,
            ITrackService trackService,
            ILikeTrackService likeTrackService,
            IMapper mapper)
        {
            _userManager = userManager;
            _playlistService = playlistService;
            _trackService = trackService;
            _likeTrackService = likeTrackService;
            _mapper = mapper;
        }

        public async Task<IActionResult> Index(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            
            var playlistDto = await _playlistService.GetPlaylistByIdAsync(id);
            if (playlistDto == null)
            {
                return NotFound();
            }

            // Security check
            if (!playlistDto.IsPublic && (user == null || playlistDto.UserId != user.Id))
            {
                return Forbid();
            }

            var tracks = await _trackService.GetTracksByPlaylistAsync(id);
            
            var likeDates = new Dictionary<int, DateTime>();
            if (user != null)
            {
                foreach (var track in tracks)
                {
                    var likeTrackDto = await _likeTrackService.GetLikeByUserAndTrackAsync(user.Id, track.TrackId);
                    if (likeTrackDto != null)
                    {
                        likeDates[track.TrackId] = likeTrackDto.LikeDate;
                    }
                }
            }

            var model = new PlaylistViewModel
            {
                Playlist = playlistDto,
                Tracks = tracks,
                LikeDates = likeDates,
                CurrentUser = _mapper.Map<UserDto>(user)
            };

            return View("~/Views/Playlist/IndexPlaylist.cshtml", model);
        }
    }
}

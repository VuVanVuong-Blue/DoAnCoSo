using Microsoft.AspNetCore.Mvc;
using System_Music.Services.Interfaces;
using System_Music.Models.ViewModels;
using System.Security.Claims;
using System_Music.Models.Common;
using System_Music.Models.SqlModels;
using System_Music.Models.DTOs;

namespace System_Music.Controllers.Web
{
    public class TrackController : Controller
    {
        private readonly ITrackService _trackService;
        private readonly IListenHistoryService _listenHistoryService;
        private readonly ISearchService _searchService;

        public TrackController(
            ITrackService trackService, 
            IListenHistoryService listenHistoryService,
            ISearchService searchService)
        {
            _trackService = trackService;
            _listenHistoryService = listenHistoryService;
            _searchService = searchService;
        }

        [HttpGet]
        [Route("Track/Detail/{id}")]
        public async Task<IActionResult> Detail(int id)
        {
            var track = await _trackService.GetTrackWithDetailsAsync(id);
            if (track == null)
            {
                return NotFound();
            }


            var recommended = await _trackService.GetRecommendedTracksAsync(id, 5);
            var viewModel = new TrackDetailViewModel 
            { 
                Track = track,
                RecommendedTracks = recommended
            };
            return View("~/Views/Playlist/TrackDetail.cshtml", viewModel);
        }

        [HttpGet]
        [Route("Track/Search")]
        public async Task<IActionResult> Search(string query)
        {
            var result = await _searchService.SearchAllAsync(query);
            return Json(new 
            {
                tracks = result.Songs.Select(s => new {
                    id = s.TrackId,
                    title = s.Title,
                    subtitle = s.Artists.FirstOrDefault()?.Name ?? "Unknown Artist",
                    image = s.ImageUrl ?? "/images/default-track.png",
                    url = $"/track/{s.TrackId}"
                }),
                artists = result.Artists.Select(a => new {
                     id = a.ArtistId,
                     title = a.Name,
                     subtitle = "Nghệ sĩ",
                     image = a.ImageUrl ?? "/images/default-artist.png",
                     url = $"/artist/{a.ArtistId}"
                }),
                albums = result.Albums.Select(al => new {
                     id = al.AlbumId,
                     title = al.Name,
                     subtitle = al.Artists.FirstOrDefault()?.Name ?? "Unknown Artist",
                     image = al.ImageUrl ?? "/images/default-album.png",
                     url = $"/album/{al.AlbumId}"
                })
            });
        }
    }
}

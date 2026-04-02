using Microsoft.AspNetCore.Mvc;
using System_Music.Services.Interfaces;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using System_Music.Models.SqlModels;

namespace System_Music.Controllers.Web
{
    [Route("Lyrics")]
    public class LyricsController : Controller
    {
        private readonly ILyricsTimingService _lyricsService;

        public LyricsController(ILyricsTimingService lyricsService)
        {
            _lyricsService = lyricsService;
        }

        [HttpGet("GetLyrics")]
        public async Task<IActionResult> GetLyrics(string trackTitle)
        {
            if (string.IsNullOrEmpty(trackTitle))
            {
                return BadRequest("Track title is required.");
            }

            var track = await _lyricsService.GetTrackByTitleAsync(trackTitle);
            if (track == null)
            {
                return NotFound("<p>Không tìm thấy bài hát.</p>");
            }

            var lyricsList = await _lyricsService.GetLyricsByTrackAsync(track.TrackId);
            ViewData["LyricsList"] = lyricsList ?? new List<LyricsTiming>();

            return View("LyricsKaraoke", track);
        }

        [HttpGet("LyricsKaraoke")]
        public async Task<IActionResult> LyricsKaraoke(string trackTitle, string audioUrl = null)
        {
            if (string.IsNullOrWhiteSpace(trackTitle))
            {
                return BadRequest("Track title is required.");
            }

            var track = await _lyricsService.GetTrackByTitleAsync(trackTitle);
            if (track == null)
            {
                return NotFound("Không tìm thấy bài hát.");
            }

            var lyricsList = await _lyricsService.GetLyricsByTrackAsync(track.TrackId);
            ViewData["LyricsList"] = lyricsList ?? new List<LyricsTiming>();
            ViewData["AudioUrl"] = audioUrl ?? track.AudioUrl;

            return View("LyricsKaraoke", track);
        }
    }
}

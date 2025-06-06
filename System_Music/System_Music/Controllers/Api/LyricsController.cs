using Microsoft.AspNetCore.Mvc;
using System_Music.Models.SqlModels;
using System_Music.Services.Interfaces;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.Text.RegularExpressions;
using System;

namespace System_Music.Controllers
{
    [Route("Lyrics")]
    public class LyricsController : Controller
    {
        private readonly ILyricsTimingService _lyricsService;
        private readonly ITempDataDictionaryFactory _tempDataFactory;

        public LyricsController(ILyricsTimingService lyricsService, ITempDataDictionaryFactory tempDataFactory)
        {
            _lyricsService = lyricsService;
            _tempDataFactory = tempDataFactory;
        }

        [HttpGet("GetLyrics")]
        public async Task<IActionResult> GetLyrics(string trackTitle)
        {
            try
            {
                if (string.IsNullOrEmpty(trackTitle))
                {
                    Console.WriteLine("GetLyrics: Track title is empty or null.");
                    return BadRequest("Track title is required.");
                }

                Console.WriteLine($"GetLyrics: Searching for track with title '{trackTitle}'");
                var track = await _lyricsService.GetTrackByTitleAsync(trackTitle);

                if (track == null)
                {
                    Console.WriteLine($"GetLyrics: No track found for title '{trackTitle}'.");
                    return NotFound("<p>Không tìm thấy bài hát.</p>");
                }

                Console.WriteLine($"GetLyrics: Found track '{track.Title}' with TrackId {track.TrackId}");
                var lyricsList = await _lyricsService.GetLyricsByTrackAsync(track.TrackId);

                var viewData = new ViewDataDictionary(new EmptyModelMetadataProvider(), new ModelStateDictionary())
                {
                    { "LyricsList", lyricsList ?? new List<LyricsTiming>() }
                };

                Console.WriteLine("GetLyrics: Attempting to render view 'LyricsKaraoke'");
                return View("LyricsKaraoke", track); // Render view trực tiếp
            }
            catch (Exception ex)
            {
                Console.WriteLine($"GetLyrics: Error - {ex.Message}\nStackTrace: {ex.StackTrace}");
                return StatusCode(500, $"<p>Lỗi khi tải lời bài hát: {ex.Message}</p>");
            }
        }

        [HttpGet("LyricsKaraoke")]
        public async Task<IActionResult> LyricsKaraoke(string trackTitle, string audioUrl = null)
        {
            Console.WriteLine($"LyricsKaraoke: Received trackTitle = {trackTitle}, audioUrl = {audioUrl}");
            if (string.IsNullOrWhiteSpace(trackTitle))
            {
                Console.WriteLine("LyricsKaraoke: Track title is empty or null.");
                return BadRequest("Track title is required.");
            }

            var normalizedTitle = Regex.Replace(
                trackTitle.Normalize(System.Text.NormalizationForm.FormD),
                "[\u0300-\u036f]",
                "",
                RegexOptions.Compiled
            ).ToLower().Trim();

            Console.WriteLine($"LyricsKaraoke: Searching for track with normalized title '{normalizedTitle}'");
            var track = await _lyricsService.GetTrackByTitleAsync(normalizedTitle);
            if (track == null)
            {
                Console.WriteLine($"LyricsKaraoke: No track found for normalized title '{normalizedTitle}'.");
                return NotFound("Không tìm thấy bài hát.");
            }

            var lyricsList = await _lyricsService.GetLyricsByTrackAsync(track.TrackId);
            Console.WriteLine($"LyricsKaraoke: Found {lyricsList?.Count ?? 0} lyrics for TrackId {track.TrackId}");
            ViewData["LyricsList"] = lyricsList ?? new List<LyricsTiming>();

            // Truyền audioUrl vào ViewData để sử dụng trong view
            ViewData["AudioUrl"] = audioUrl ?? track.AudioUrl; // Sử dụng audioUrl từ query hoặc từ track nếu có
            Console.WriteLine("LyricsKaraoke: Rendering view...");
            return View("LyricsKaraoke", track); // Render view trực tiếp
        }
    }
}
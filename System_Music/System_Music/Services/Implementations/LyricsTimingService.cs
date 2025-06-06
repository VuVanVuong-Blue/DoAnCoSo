using System.Text.RegularExpressions;
using System;
using System_Music.Models.SqlModels;
using System_Music.Repositories.Interfaces;
using System_Music.Services.Interfaces;

namespace System_Music.Services.Implementations
{
    public class LyricsTimingService : ILyricsTimingService
    {
        private readonly ILyricsTimingRepository _lyricsTimingRepository;
        private readonly ITrackService _trackService;

        public LyricsTimingService(ILyricsTimingRepository lyricsTimingRepository, ITrackService trackService)
        {
            _lyricsTimingRepository = lyricsTimingRepository;
            _trackService = trackService;
        }

        public async Task<List<LyricsTiming>> GetAllLyricsAsync()
        {
            return await _lyricsTimingRepository.GetAllAsync();
        }

        public async Task<LyricsTiming> GetLyricByIdAsync(int id)
        {
            return await _lyricsTimingRepository.GetByIdAsync(id);
        }

        public async Task AddLyricAsync(LyricsTiming lyricsTiming)
        {
            await _lyricsTimingRepository.AddAsync(lyricsTiming);
        }

        public async Task UpdateLyricAsync(LyricsTiming lyricsTiming)
        {
            await _lyricsTimingRepository.UpdateAsync(lyricsTiming);
        }

        public async Task DeleteLyricAsync(int id)
        {
            await _lyricsTimingRepository.DeleteAsync(id);
        }

        public async Task<List<LyricsTiming>> GetLyricsByTrackAsync(int trackId)
        {
            var lyrics = await _lyricsTimingRepository.GetLyricsByTrackAsync(trackId);
            Console.WriteLine($"GetLyricsByTrackAsync: Found {lyrics?.Count ?? 0} lyrics for TrackId {trackId}");
            return lyrics ?? new List<LyricsTiming>(); // Trả về danh sách rỗng nếu null
        }

        public async Task<Track> GetTrackByTitleAsync(string trackTitle)
        {
            if (string.IsNullOrWhiteSpace(trackTitle))
            {
                Console.WriteLine("GetTrackByTitleAsync: Track title is empty or null.");
                return null;
            }

            try
            {
                // Sử dụng Regex.Replace để loại bỏ dấu thay vì Replace với RegexOptions
                var normalizedTitle = Regex.Replace(
                    trackTitle.Normalize(System.Text.NormalizationForm.FormD),
                    @"\p{IsCombiningDiacriticalMarks}+",
                    string.Empty
                ).ToLower().Trim();
                Console.WriteLine($"GetTrackByTitleAsync: Searching for normalized title '{normalizedTitle}'");
                var track = await _trackService.GetTrackByNormalizedTitleAsync(normalizedTitle);
                if (track == null)
                {
                    Console.WriteLine($"GetTrackByTitleAsync: No track found for normalized title '{normalizedTitle}'.");
                }
                else
                {
                    Console.WriteLine($"GetTrackByTitleAsync: Found track '{track.Title}' with NormalizedTitle '{track.NormalizedTitle}' and TrackId {track.TrackId}");
                }
                return track;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"GetTrackByTitleAsync: Error fetching track '{trackTitle}'. Exception: {ex.Message}\nStackTrace: {ex.StackTrace}");
                return null;
            }
        }

        public async Task<Track> GetTrackByNormalizedTitleAsync(string normalizedTitle)
        {
            if (string.IsNullOrWhiteSpace(normalizedTitle))
            {
                Console.WriteLine("GetTrackByNormalizedTitleAsync: Normalized title is empty or null.");
                return null;
            }

            try
            {
                Console.WriteLine($"GetTrackByNormalizedTitleAsync: Searching for normalized title '{normalizedTitle}'.");
                var track = await _trackService.GetTrackByNormalizedTitleAsync(normalizedTitle); // Gọi trực tiếp
                if (track == null)
                {
                    Console.WriteLine($"GetTrackByNormalizedTitleAsync: No track found for normalized title '{normalizedTitle}'.");
                }
                else
                {
                    Console.WriteLine($"GetTrackByNormalizedTitleAsync: Found track '{track.Title}' with NormalizedTitle '{track.NormalizedTitle}' and TrackId {track.TrackId}");
                }
                return track;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"GetTrackByNormalizedTitleAsync: Error fetching track '{normalizedTitle}'. Exception: {ex.Message}\nStackTrace: {ex.StackTrace}");
                return null;
            }
        }
    }
}
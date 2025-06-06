using Microsoft.EntityFrameworkCore;
using System_Music.Models.SqlModels;
using System_Music.Services.Interfaces;

namespace System_Music.Services.Implementations
{
    public class ChartRankingService : IChartRankingService
    {
        private readonly SmartMusicDbContext _context;

        public ChartRankingService(SmartMusicDbContext context)
        {
            _context = context;
        }

        public async Task<List<ChartRanking>> GetTopTracksAsync(string country, string timeFrame, int limit = 50)
        {
            var rankings = await _context.ChartRankings
                .Where(cr => cr.Country == country && cr.TimeFrame == timeFrame)
                .OrderBy(cr => cr.RankPosition)
                .Take(limit)
                .Include(cr => cr.Track)
                    .ThenInclude(t => t.Album)
                .Include(cr => cr.Track)
                    .ThenInclude(t => t.TrackArtists)
                    .ThenInclude(ta => ta.Artist)
                .AsNoTracking()
                .ToListAsync();
            return rankings ?? new List<ChartRanking>();
        }

        public async Task UpdateChartRankingAsync(string country, string timeFrame)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // Xóa bảng xếp hạng cũ
                var oldRankings = _context.ChartRankings
                    .Where(cr => cr.Country == country && cr.TimeFrame == timeFrame);
                _context.ChartRankings.RemoveRange(oldRankings);
                await _context.SaveChangesAsync();

                // Lấy top 50 bài hát dựa trên PlayCount và LikeCount từ Track
                var tracks = await _context.Tracks
                    .OrderByDescending(t => t.PlayCount * 0.7 + t.LikeCount * 0.3)
                    .Take(50)
                    .ToListAsync();

                // Tạo bảng xếp hạng mới
                var rankings = new List<ChartRanking>();
                for (int i = 0; i < tracks.Count; i++)
                {
                    var track = tracks[i];
                    var ranking = new ChartRanking
                    {
                        TrackId = track.TrackId,
                        RankPosition = i + 1,
                        TrendScore = (float)(track.PlayCount * 0.7 + track.LikeCount * 0.3),
                        TotalPlays = track.PlayCount,
                        TotalLikes = track.LikeCount,
                        Date = DateTime.UtcNow,
                        TimeFrame = timeFrame,
                        Country = country,
                        Title = $"50 bài hát hàng đầu tại {country}",
                        Description = $"Thông tin cập nhật hằng {timeFrame} về những bản nhạc được nghe nhiều nhất hiện nay tại {country}.",
                        ImageUrl = "https://via.placeholder.com/200" // Admin có thể thay đổi
                    };
                    rankings.Add(ranking);
                }

                _context.ChartRankings.AddRange(rankings);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                throw new Exception($"Error updating chart ranking: {ex.Message}", ex);
            }
        }
    }
}
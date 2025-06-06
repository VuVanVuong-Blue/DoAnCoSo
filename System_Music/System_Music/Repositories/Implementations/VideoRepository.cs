using Microsoft.EntityFrameworkCore;
using System_Music.Models.SqlModels;
using System_Music.Interfaces;
using System.Threading.Tasks;

namespace System_Music.Repositories.Implementations
{
    public class VideoRepository : IVideoRepository
    {
        private readonly SmartMusicDbContext _context;

        public VideoRepository(SmartMusicDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<Video> GetVideoByIdAsync(string encodeId)
        {
            return await _context.Videos
                .Include(v => v.VideoArtists)
                .ThenInclude(va => va.Artist)
                .FirstOrDefaultAsync(v => v.EncodeId == encodeId);
        }

        public async Task AddVideoAsync(Video video)
        {
            _context.Videos.Add(video);
            await _context.SaveChangesAsync();
        }

        public async Task AddVideoArtistsAsync(List<VideoArtist> videoArtists)
        {
            _context.VideoArtists.AddRange(videoArtists);
            await _context.SaveChangesAsync();
        }
        public async Task AddOrUpdateVideoAsync(Video video)
        {
            var existingVideo = await _context.Videos.FindAsync(video.EncodeId);
            if (existingVideo == null)
            {
                _context.Videos.Add(video);
            }
            else
            {
                _context.Entry(existingVideo).CurrentValues.SetValues(video);
            }
            await _context.SaveChangesAsync();
        }
        public async Task DeleteVideoAsync(string encodeId)
        {
            var video = await _context.Videos.FindAsync(encodeId);
            if (video != null)
            {
                _context.Videos.Remove(video);
                await _context.SaveChangesAsync();
            }
        }
    }
}
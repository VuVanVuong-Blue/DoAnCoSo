using Microsoft.EntityFrameworkCore;
using System_Music.Models.SqlModels;
using System_Music.Repositories.Interfaces;

namespace System_Music.Repositories.Implementations
{
    public class ArtistRepository : Repository<Artist>, IArtistRepository
    {
        private readonly SmartMusicDbContext _context;

        public ArtistRepository(SmartMusicDbContext context) : base(context)
        {
            _context = context;
        }

        public override async Task<List<Artist>> GetAllAsync()
        {
            return await _context.Artists
                .Include(a => a.TrackArtists)
                    .ThenInclude(ta => ta.Track)
                .Include(a => a.AlbumArtists)
                    .ThenInclude(aa => aa.Album)
                .ToListAsync();
        }

        public Task<List<Artist>> GetArtistsByCountryAsync(string country)
        {
            return _context.Artists
                 .Where(a => a.Country == country)
                 .Include(a => a.TrackArtists)
                     .ThenInclude(ta => ta.Track)
                 .Include(a => a.AlbumArtists)
                     .ThenInclude(aa => aa.Album)
                 .ToListAsync();
        }

        public  async Task<Artist> GetByIdAsync(int id)
        {
            return await _context.Artists
                .Include(a => a.TrackArtists)
                    .ThenInclude(ta => ta.Track)
                .Include(a => a.AlbumArtists)
                    .ThenInclude(aa => aa.Album)
                .FirstOrDefaultAsync(a => a.ArtistId == id);
        }

        public async Task<Artist> GetByIdWithDetailsAsync(int id)
        {
            return await _context.Artists
                .Include(a => a.TrackArtists)
                    .ThenInclude(ta => ta.Track)
                        .ThenInclude(t => t.Album)
                .Include(a => a.TrackArtists)
                    .ThenInclude(ta => ta.Track)
                        .ThenInclude(t => t.TrackGenres)
                            .ThenInclude(tg => tg.Genre)
                .Include(a => a.AlbumArtists)
                    .ThenInclude(aa => aa.Album)
                        .ThenInclude(album => album.Tracks)
                .FirstOrDefaultAsync(a => a.ArtistId == id);
        }
    }
}
using Microsoft.EntityFrameworkCore;
using System_Music.Models.SqlModels;
using System_Music.Repositories.Interfaces;

namespace System_Music.Repositories.Implementations
{
    public class ArtistRepository : Repository<Artist>, IArtistRepository
    {
        public ArtistRepository(SmartMusicDbContext context) : base(context) { }

        public override async Task<List<Artist>> GetAllAsync()
        {
            return await _context.Artists
                .Include(a => a.TrackArtists)
                    .ThenInclude(ta => ta.Track)
                .ToListAsync();
        }

        public async Task<List<Artist>> GetArtistsByCountryAsync(string country)
        {
            return await _context.Artists
                 .Where(a => a.Country == country)
                 .ToListAsync();
        }

        public override async Task<Artist> GetByIdAsync<TId>(TId id)
        {
            return await _context.Artists
                .FirstOrDefaultAsync(a => a.ArtistId.Equals(id));
        }

        public async Task<Artist> GetByIdWithDetailsAsync(int id)
        {
            return await _context.Artists
                .Include(a => a.TrackArtists)
                    .ThenInclude(ta => ta.Track)
                .Include(a => a.AlbumArtists)
                    .ThenInclude(aa => aa.Album)
                .FirstOrDefaultAsync(a => a.ArtistId == id);
        }

        public async Task<List<Artist>> GetArtistsBySearchAsync(string searchTerm)
        {
            return await _context.Artists
                .Where(a => a.Name.Contains(searchTerm))
                .ToListAsync();
        }
    }
}
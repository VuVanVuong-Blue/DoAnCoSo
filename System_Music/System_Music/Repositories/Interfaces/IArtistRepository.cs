using System_Music.Models.SqlModels;

namespace System_Music.Repositories.Interfaces
{
    public interface IArtistRepository : IRepository<Artist>
    {
        Task<List<Artist>> GetArtistsByCountryAsync(string country);
        Task<Artist> GetByIdWithDetailsAsync(int id);
    }
}
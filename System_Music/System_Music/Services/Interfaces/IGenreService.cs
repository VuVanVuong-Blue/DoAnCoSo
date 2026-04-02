using System_Music.Models.DTOs;
using System_Music.Models.SqlModels;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace System_Music.Services.Interfaces
{
    public interface IGenreService
    {
        Task<List<GenreDto>> GetAllGenresAsync();
        Task<GenreDto> GetGenreByIdAsync(int id);
        Task<GenreDto> GetGenreWithDetailsAsync(int id);
        Task AddGenreAsync(GenreDto genreDto);
        Task UpdateGenreAsync(GenreDto genreDto);
        Task DeleteGenreAsync(int id);
        Task<List<GenreDto>> GetGenresByTrackIdAsync(int trackId);
        Task<bool> GenreExistsAsync(int genreId);
        Task<List<GenreDto>> SyncGenresFromZingMp3Async();
    }
}
using System_Music.Models.DTOs;

namespace System_Music.Services.Interfaces
{
    public interface ISearchService
    {
        Task<SearchResultDto> SearchAllAsync(string searchTerm);
    }
}

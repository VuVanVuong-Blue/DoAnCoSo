using AutoMapper;
using System_Music.Models.DTOs;
using System_Music.Models.SqlModels;
using System_Music.Repositories.Interfaces;
using System_Music.Services.Interfaces;

namespace System_Music.Services.Implementations
{
    public class SearchService : ISearchService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public SearchService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<SearchResultDto> SearchAllAsync(string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
                return new SearchResultDto();

            // Here we use the repositories to search.
            // Note: We might need more specialized search methods in repositories for better performance.
            // But following the current logic:
            
            var tracksTask = _unitOfWork.Tracks.GetTracksBySearchAsync(searchTerm);
            var albumsTask = _unitOfWork.Albums.GetAlbumsBySearchAsync(searchTerm);
            var artistsTask = _unitOfWork.Artists.GetArtistsBySearchAsync(searchTerm);

            await Task.WhenAll(tracksTask, albumsTask, artistsTask);

            return new SearchResultDto
            {
                Songs = _mapper.Map<List<TrackDto>>(tracksTask.Result),
                Albums = _mapper.Map<List<AlbumDto>>(albumsTask.Result),
                Artists = _mapper.Map<List<ArtistDto>>(artistsTask.Result)
            };
        }
    }
}
using System_Music.Models.DTOs;

namespace System_Music.Services.Interfaces
{
    public interface ISearchService
    {
        Task<SearchResultDto> SearchAllAsync(string searchTerm);
    }
}

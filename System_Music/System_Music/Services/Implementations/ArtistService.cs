using AutoMapper;
using System_Music.Models.DTOs;
using System_Music.Models.SqlModels;
using System_Music.Repositories.Interfaces;
using System_Music.Services.Interfaces;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Net.Http;
using System.Text.Json;
using Microsoft.Extensions.Configuration;

namespace System_Music.Services.Implementations
{
    public class ArtistService : IArtistService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly HttpClient _httpClient;
        private readonly string _zingMp3ApiBaseUrl;

        public ArtistService(IUnitOfWork unitOfWork, IMapper mapper, IHttpClientFactory httpClientFactory, IConfiguration configuration)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _httpClient = httpClientFactory.CreateClient();
            _zingMp3ApiBaseUrl = configuration["ZingMp3Api:BaseUrl"];
        }

        public async Task<List<ArtistDto>> GetAllArtistsAsync()
        {
            var artists = await _unitOfWork.Artists.GetAllAsync();
            return _mapper.Map<List<ArtistDto>>(artists);
        }

        public async Task<ArtistDto> GetArtistByIdAsync(int id)
        {
            var artist = await _unitOfWork.Artists.GetByIdAsync(id);
            return _mapper.Map<ArtistDto>(artist);
        }

        public async Task<ArtistDto> GetArtistByIdWithDetailsAsync(int id)
        {
            var artist = await _unitOfWork.Artists.GetByIdWithDetailsAsync(id);
            return _mapper.Map<ArtistDto>(artist);
        }

        public async Task<List<ArtistDto>> GetArtistsByCountryAsync(string country)
        {
            var artists = await _unitOfWork.Artists.GetArtistsByCountryAsync(country);
            return _mapper.Map<List<ArtistDto>>(artists);
        }

        public async Task<List<TrackDto>> GetTracksByArtistIdAsync(int artistId)
        {
            var artistWithTracks = await _unitOfWork.Artists.GetByIdWithDetailsAsync(artistId);
            return _mapper.Map<List<TrackDto>>(artistWithTracks?.TrackArtists.Select(ta => ta.Track));
        }

        public async Task AddArtistAsync(ArtistDto artistDto)
        {
            var artist = _mapper.Map<Artist>(artistDto);
            await _unitOfWork.Artists.AddAsync(artist);
            await _unitOfWork.CompleteAsync();
            artistDto.ArtistId = artist.ArtistId;
        }

        public async Task UpdateArtistAsync(ArtistDto artistDto)
        {
            var artist = await _unitOfWork.Artists.GetByIdAsync(artistDto.ArtistId);
            if (artist != null)
            {
                _mapper.Map(artistDto, artist);
                await _unitOfWork.Artists.UpdateAsync(artist);
                await _unitOfWork.CompleteAsync();
            }
        }

        public async Task DeleteArtistAsync(int id)
        {
            await _unitOfWork.Artists.DeleteAsync(id);
            await _unitOfWork.CompleteAsync();
        }

        public async Task<IEnumerable<ArtistDto>> SyncArtistsFromZingMp3Async(string artistName, string artistId)
        {
            // Placeholder/Stub logic if the original was missing.
            // In a real scenario, this would call the ZingMp3 API and update the database.
            // For now, return existing artists if sync is not available.
            return await GetAllArtistsAsync();
        }
    }
}
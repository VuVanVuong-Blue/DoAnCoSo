using AutoMapper;
using Newtonsoft.Json;
using System_Music.Models.DTOs;
using System_Music.Models.SqlModels;
using System_Music.Repositories.Interfaces;
using System_Music.Services.Interfaces;
using System.Net.Http;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using System.Linq;

namespace System_Music.Services.Implementations
{
    public class GenreService : IGenreService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly HttpClient _httpClient;
        private readonly string _zingMp3ApiUrl;

        public GenreService(IUnitOfWork unitOfWork, IMapper mapper, HttpClient httpClient, IConfiguration configuration)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _httpClient = httpClient;
            _zingMp3ApiUrl = configuration["ZingMp3Api:BaseUrl"] ?? "http://localhost:5000";
        }

        public async Task<List<GenreDto>> GetAllGenresAsync()
        {
            var genres = await _unitOfWork.Genres.GetAllAsync();
            return _mapper.Map<List<GenreDto>>(genres);
        }

        public async Task<GenreDto> GetGenreByIdAsync(int id)
        {
            var genre = await _unitOfWork.Genres.GetByIdAsync(id);
            if (genre == null) throw new KeyNotFoundException($"Genre with ID {id} not found.");
            return _mapper.Map<GenreDto>(genre);
        }

        public async Task<GenreDto> GetGenreWithDetailsAsync(int id)
        {
            var genre = await _unitOfWork.Genres.GetByIdWithDetailsAsync(id);
            if (genre == null) throw new KeyNotFoundException($"Genre with ID {id} not found.");
            return _mapper.Map<GenreDto>(genre);
        }

        public async Task AddGenreAsync(GenreDto genreDto)
        {
            var genre = _mapper.Map<Genre>(genreDto);
            var existingGenre = await _unitOfWork.Genres.GetAsync(g => g.Name == genre.Name);
            if (existingGenre != null) throw new InvalidOperationException($"Genre with name '{genre.Name}' already exists.");

            await _unitOfWork.Genres.AddAsync(genre);
            await _unitOfWork.CompleteAsync();
            genreDto.GenreId = genre.GenreId;
        }

        public async Task UpdateGenreAsync(GenreDto genreDto)
        {
            var genre = await _unitOfWork.Genres.GetByIdAsync(genreDto.GenreId);
            if (genre == null) throw new KeyNotFoundException($"Genre with ID {genreDto.GenreId} not found.");

            _mapper.Map(genreDto, genre);
            await _unitOfWork.Genres.UpdateAsync(genre);
            await _unitOfWork.CompleteAsync();
        }

        public async Task DeleteGenreAsync(int id)
        {
            await _unitOfWork.Genres.DeleteAsync(id);
            await _unitOfWork.CompleteAsync();
        }

        public async Task<List<GenreDto>> GetGenresByTrackIdAsync(int trackId)
        {
            var genres = await _unitOfWork.Genres.GetGenresByTrackIdAsync(trackId);
            return _mapper.Map<List<GenreDto>>(genres);
        }

        public async Task<bool> GenreExistsAsync(int genreId)
        {
            return await _unitOfWork.Genres.GenreExistsAsync(genreId);
        }

        public async Task<List<GenreDto>> SyncGenresFromZingMp3Async()
        {
            try
            {
                var response = await _httpClient.GetAsync($"{_zingMp3ApiUrl}/api/genres");
                response.EnsureSuccessStatusCode();

                var jsonString = await response.Content.ReadAsStringAsync();
                var apiResponse = JsonConvert.DeserializeObject<ZingMp3ApiResponse>(jsonString);

                if (apiResponse.Err != 0 || apiResponse.Data?.Genres == null)
                    throw new Exception($"Error from Zing MP3 API: {apiResponse.Msg}");

                var zingGenres = apiResponse.Data.Genres;
                var existingGenres = await _unitOfWork.Genres.GetAllAsync();

                foreach (var zingGenre in zingGenres)
                {
                    var existingGenre = existingGenres.FirstOrDefault(g => g.ZingMp3GenreId == zingGenre.Id);
                    if (existingGenre == null)
                    {
                        var newGenre = new Genre
                        {
                            Name = zingGenre.Name,
                            Description = zingGenre.Title,
                            ZingMp3GenreId = zingGenre.Id
                        };
                        await _unitOfWork.Genres.AddAsync(newGenre);
                    }
                    else
                    {
                        existingGenre.Name = zingGenre.Name;
                        existingGenre.Description = zingGenre.Title;
                        await _unitOfWork.Genres.UpdateAsync(existingGenre);
                    }
                }

                await _unitOfWork.CompleteAsync();
                return await GetAllGenresAsync();
            }
            catch (Exception ex)
            {
                throw new Exception($"Error syncing genres: {ex.Message}", ex);
            }
        }

        private class ZingMp3ApiResponse
        {
            public int Err { get; set; }
            public string Msg { get; set; }
            public ZingMp3Data Data { get; set; }
        }

        private class ZingMp3Data
        {
            public List<ZingMp3Genre> Genres { get; set; }
        }

        private class ZingMp3Genre
        {
            public string Id { get; set; }
            public string Name { get; set; }
            public string Title { get; set; }
        }
    }
}
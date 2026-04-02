using AutoMapper;
using Microsoft.EntityFrameworkCore;
using System_Music.Models.DTOs;
using System_Music.Models.SqlModels;
using System_Music.Repositories.Interfaces;
using System_Music.Services.Interfaces;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace System_Music.Services.Implementations
{
    public class TrackService : ITrackService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public TrackService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<List<TrackDto>> GetAllTracksAsync()
        {
            var tracks = await _unitOfWork.Tracks.GetAllAsync();
            return _mapper.Map<List<TrackDto>>(tracks);
        }

        public async Task<TrackDto> GetTrackByIdAsync(int id)
        {
            var track = await _unitOfWork.Tracks.GetByIdAsync(id);
            return _mapper.Map<TrackDto>(track);
        }

        public async Task<TrackDto> GetTrackWithDetailsAsync(int id)
        {
            var track = await _unitOfWork.Tracks.GetByIdWithDetailsAsync(id);
            return _mapper.Map<TrackDto>(track);
        }

        public async Task AddTrackAsync(TrackDto trackDto, int[] artistIds, int[] genreIds)
        {
            var track = _mapper.Map<Track>(trackDto);
            await _unitOfWork.Tracks.AddAsync(track);
            await _unitOfWork.CompleteAsync();

            if (artistIds != null)
            {
                foreach (var artistId in artistIds)
                {
                    await _unitOfWork.TrackArtists.AddAsync(new TrackArtist { TrackId = track.TrackId, ArtistId = artistId });
                }
            }

            if (genreIds != null)
            {
                foreach (var genreId in genreIds)
                {
                    await _unitOfWork.TrackGenres.AddAsync(new TrackGenre { TrackId = track.TrackId, GenreId = genreId });
                }
            }

            await _unitOfWork.CompleteAsync();
            trackDto.TrackId = track.TrackId;
        }

        public async Task UpdateTrackAsync(TrackDto trackDto, int[] artistIds, int[] genreIds)
        {
            var track = await _unitOfWork.Tracks.GetByIdWithDetailsAsync(trackDto.TrackId);
            if (track != null)
            {
                _mapper.Map(trackDto, track);
                await _unitOfWork.Tracks.UpdateAsync(track);

                // Update Artists
                var currentArtistIds = track.TrackArtists.Select(ta => ta.ArtistId).ToList();
                var newArtistIds = artistIds ?? new int[0];

                foreach (var oldId in currentArtistIds.Where(id => !newArtistIds.Contains(id)))
                {
                    var ta = track.TrackArtists.First(x => x.ArtistId == oldId);
                    await _unitOfWork.TrackArtists.DeleteAsync(ta.TrackId, ta.ArtistId);
                }

                foreach (var newId in newArtistIds.Where(id => !currentArtistIds.Contains(id)))
                {
                    await _unitOfWork.TrackArtists.AddAsync(new TrackArtist { TrackId = track.TrackId, ArtistId = newId });
                }

                // Update Genres
                var currentGenreIds = track.TrackGenres.Select(tg => tg.GenreId).ToList();
                var newGenreIds = genreIds ?? new int[0];

                foreach (var oldId in currentGenreIds.Where(id => !newGenreIds.Contains(id)))
                {
                    var tg = track.TrackGenres.First(x => x.GenreId == oldId);
                    await _unitOfWork.TrackGenres.DeleteAsync(tg.TrackId, tg.GenreId);
                }

                foreach (var newId in newGenreIds.Where(id => !currentGenreIds.Contains(id)))
                {
                    await _unitOfWork.TrackGenres.AddAsync(new TrackGenre { TrackId = track.TrackId, GenreId = newId });
                }

                await _unitOfWork.CompleteAsync();
            }
        }

        public async Task DeleteTrackAsync(int id)
        {
            await _unitOfWork.Tracks.DeleteAsync(id);
            await _unitOfWork.CompleteAsync();
        }

        public async Task<List<TrackDto>> GetTopTracksAsync(int count)
        {
            var tracks = await _unitOfWork.Tracks.GetTopTracksAsync(count);
            return _mapper.Map<List<TrackDto>>(tracks);
        }

        public async Task<List<TrackDto>> GetTracksByAlbumAsync(int albumId)
        {
            var tracks = await _unitOfWork.Tracks.GetTracksByAlbumAsync(albumId);
            return _mapper.Map<List<TrackDto>>(tracks);
        }

        public async Task<List<TrackDto>> SearchTracksAsync(string searchTerm)
        {
            var tracks = await _unitOfWork.Tracks.GetTracksBySearchAsync(searchTerm);
            return _mapper.Map<List<TrackDto>>(tracks);
        }

        public async Task<List<TrackDto>> GetTracksByPlaylistAsync(int playlistId)
        {
            var tracks = await _unitOfWork.Tracks.GetTracksByPlaylistAsync(playlistId);
            return _mapper.Map<List<TrackDto>>(tracks);
        }

        public async Task<List<TrackDto>> SyncTracksFromZingMp3Async(string encodeId)
        {
            var tracks = await _unitOfWork.Tracks.SyncTracksFromZingMp3Async(encodeId);
            await _unitOfWork.CompleteAsync();
            return _mapper.Map<List<TrackDto>>(tracks);
        }

        public async Task<List<TrackDto>> SyncArtistSongsFromZingMp3Async(string artistId, int page = 1, int count = 5)
        {
            var tracks = await _unitOfWork.Tracks.SyncArtistSongsFromZingMp3Async(artistId, page, count);
            await _unitOfWork.CompleteAsync();
            return _mapper.Map<List<TrackDto>>(tracks);
        }

        public async Task<List<TrackDto>> GetLikedTracksAsync(string userId)
        {
            var likes = await _unitOfWork.Tracks.GetLikesByUserAsync(userId);
            return _mapper.Map<List<TrackDto>>(likes.Select(l => l.Track).ToList());
        }

        public async Task<TrackDto> GetTrackByTitleAsync(string trackTitle)
        {
            var track = await _unitOfWork.Tracks.GetTrackByTitleAsync(trackTitle);
            return _mapper.Map<TrackDto>(track);
        }

        public async Task<TrackDto> GetTrackByNormalizedTitleAsync(string normalizedTitle)
        {
            var track = await _unitOfWork.Tracks.GetTrackByNormalizedTitleAsync(normalizedTitle);
            return _mapper.Map<TrackDto>(track);
        }

        public async Task<List<TrackDto>> GetRecommendedTracksAsync(int trackId, int count)
        {
            var track = await _unitOfWork.Tracks.GetByIdWithDetailsAsync(trackId);
            if (track == null) return new List<TrackDto>();

            var genreIds = track.TrackGenres.Select(tg => tg.GenreId).ToList();
            var recommended = await _unitOfWork.Tracks.GetTracksByGenresAsync(genreIds, trackId, count);
            return _mapper.Map<List<TrackDto>>(recommended);
        }
    }
}
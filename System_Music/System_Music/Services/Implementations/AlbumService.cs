using AutoMapper;
using System_Music.Models.DTOs;
using System_Music.Models.SqlModels;
using System_Music.Repositories.Interfaces;
using System_Music.Services.Interfaces;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace System_Music.Services.Implementations
{
    public class AlbumService : IAlbumService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public AlbumService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<List<AlbumDto>> GetAllAlbumsAsync()
        {
            var albums = await _unitOfWork.Albums.GetAllAsync();
            return _mapper.Map<List<AlbumDto>>(albums);
        }

        public async Task<AlbumDto> GetAlbumByIdAsync(int id)
        {
            var album = await _unitOfWork.Albums.GetByIdAsync(id);
            return _mapper.Map<AlbumDto>(album);
        }

        public async Task<AlbumDto> GetAlbumByIdWithDetailsAsync(int id)
        {
            var album = await _unitOfWork.Albums.GetByIdWithDetailsAsync(id);
            return _mapper.Map<AlbumDto>(album);
        }

        public async Task<List<AlbumDto>> GetAlbumsByArtistAsync(int artistId)
        {
            var albums = await _unitOfWork.Albums.GetAlbumsByArtistAsync(artistId);
            return _mapper.Map<List<AlbumDto>>(albums);
        }

        public async Task AddAlbumAsync(AlbumDto albumDto, int[] artistIds, int[] trackIds)
        {
            var album = _mapper.Map<Album>(albumDto);
            await _unitOfWork.Albums.AddAsync(album);
            await _unitOfWork.CompleteAsync();

            // Link Artists
            if (artistIds != null)
            {
                foreach (var artistId in artistIds)
                {
                    await _unitOfWork.AlbumArtists.AddAsync(new AlbumArtist { AlbumId = album.AlbumId, ArtistId = artistId });
                }
            }

            // Link Tracks
            if (trackIds != null)
            {
                foreach (var trackId in trackIds)
                {
                    var track = await _unitOfWork.Tracks.GetByIdAsync(trackId);
                    if (track != null)
                    {
                        track.AlbumId = album.AlbumId;
                        await _unitOfWork.Tracks.UpdateAsync(track);
                    }
                }
            }

            await _unitOfWork.CompleteAsync();
            albumDto.AlbumId = album.AlbumId;
        }

        public async Task UpdateAlbumAsync(AlbumDto albumDto, int[] artistIds, int[] trackIds)
        {
            var album = await _unitOfWork.Albums.GetByIdWithDetailsAsync(albumDto.AlbumId);
            if (album != null)
            {
                _mapper.Map(albumDto, album);
                await _unitOfWork.Albums.UpdateAsync(album);

                // Update Artists
                var currentArtistIds = album.AlbumArtists.Select(aa => aa.ArtistId).ToList();
                var newArtistIds = artistIds ?? new int[0];

                // Remove old
                foreach (var oldId in currentArtistIds.Where(id => !newArtistIds.Contains(id)))
                {
                    var aa = album.AlbumArtists.First(x => x.ArtistId == oldId);
                    await _unitOfWork.AlbumArtists.DeleteAsync(aa.AlbumId, aa.ArtistId);
                }

                // Add new
                foreach (var newId in newArtistIds.Where(id => !currentArtistIds.Contains(id)))
                {
                    await _unitOfWork.AlbumArtists.AddAsync(new AlbumArtist { AlbumId = album.AlbumId, ArtistId = newId });
                }

                // Update Tracks
                var currentTrackIds = album.Tracks.Select(t => t.TrackId).ToList();
                var newTrackIds = trackIds ?? new int[0];

                // Unlink old
                foreach (var oldTrackId in currentTrackIds.Where(id => !newTrackIds.Contains(id)))
                {
                    var track = album.Tracks.First(t => t.TrackId == oldTrackId);
                    track.AlbumId = null;
                    await _unitOfWork.Tracks.UpdateAsync(track);
                }

                // Link new
                foreach (var newTrackId in newTrackIds.Where(id => !currentTrackIds.Contains(id)))
                {
                    var track = await _unitOfWork.Tracks.GetByIdAsync(newTrackId);
                    if (track != null)
                    {
                        track.AlbumId = album.AlbumId;
                        await _unitOfWork.Tracks.UpdateAsync(track);
                    }
                }

                await _unitOfWork.CompleteAsync();
            }
        }

        public async Task DeleteAlbumAsync(int id)
        {
            await _unitOfWork.Albums.DeleteAsync(id);
            await _unitOfWork.CompleteAsync();
        }

        public async Task<IEnumerable<AlbumDto>> SyncAlbumFromZingMp3Async(string albumEncodeId)
        {
            var albums = await _unitOfWork.Albums.SyncAlbumFromZingMp3Async(albumEncodeId);
            await _unitOfWork.CompleteAsync();
            return _mapper.Map<List<AlbumDto>>(albums);
        }
    }
}
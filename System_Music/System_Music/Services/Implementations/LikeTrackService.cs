using AutoMapper;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System_Music.Models.DTOs;
using System_Music.Models.SqlModels;
using System_Music.Repositories.Interfaces;
using System_Music.Services.Interfaces;

namespace System_Music.Services.Implementations
{
    public class LikeTrackService : ILikeTrackService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public LikeTrackService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<LikeTrackDto> GetLikeByUserAndTrackAsync(string userId, int trackId)
        {
            var likes = await _unitOfWork.LikeTracks.GetLikesByUserAsync(userId);
            var likeTrack = likes.FirstOrDefault(lt => lt.TrackId == trackId);
            return _mapper.Map<LikeTrackDto>(likeTrack);
        }

        public async Task<List<LikeTrackDto>> GetAllLikesAsync()
        {
            var likes = await _unitOfWork.LikeTracks.GetAllAsync();
            return _mapper.Map<List<LikeTrackDto>>(likes);
        }

        public async Task<LikeTrackDto> GetLikeByIdAsync(int id)
        {
            var like = await _unitOfWork.LikeTracks.GetByIdAsync(id);
            return _mapper.Map<LikeTrackDto>(like);
        }

        public async Task AddLikeAsync(LikeTrackDto likeTrackDto)
        {
            var likeTrack = _mapper.Map<LikeTrack>(likeTrackDto);
            await _unitOfWork.LikeTracks.AddAsync(likeTrack);
            await _unitOfWork.CompleteAsync();
            likeTrackDto.LikeTrackId = likeTrack.LikeTrackId;
        }

        public async Task DeleteLikeAsync(int id)
        {
            await _unitOfWork.LikeTracks.DeleteAsync(id);
            await _unitOfWork.CompleteAsync();
        }

        public async Task<List<LikeTrackDto>> GetLikesByUserAsync(string userId)
        {
            var likes = await _unitOfWork.LikeTracks.GetLikesByUserAsync(userId);
            return _mapper.Map<List<LikeTrackDto>>(likes);
        }

        public async Task<List<LikeTrackDto>> GetLikesByTrackAsync(int trackId)
        {
            var likes = await _unitOfWork.LikeTracks.GetLikesByTrackAsync(trackId);
            return _mapper.Map<List<LikeTrackDto>>(likes);
        }

        public async Task<bool> HasLikedTrackAsync(string userId, int trackId)
        {
            return await _unitOfWork.LikeTracks.HasUserLikedTrackAsync(userId, trackId);
        }
    }
}

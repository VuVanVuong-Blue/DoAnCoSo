using AutoMapper;
using Microsoft.EntityFrameworkCore;
using System_Music.Models.DTOs;
using System_Music.Models.SqlModels;
using System_Music.Repositories.Interfaces;
using System_Music.Services.Interfaces;

namespace System_Music.Services.Implementations
{
    public class ListenHistoryService : IListenHistoryService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public ListenHistoryService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<List<ListenHistoryDto>> GetAllListenHistoriesAsync()
        {
            var histories = await _unitOfWork.ListenHistories.GetAllAsync();
            return _mapper.Map<List<ListenHistoryDto>>(histories);
        }

        public async Task<List<ListenHistoryDto>> GetListenHistoriesByUserAsync(string userId)
        {
            var histories = await _unitOfWork.ListenHistories.GetByUserAsync(userId);
            return _mapper.Map<List<ListenHistoryDto>>(histories);
        }

        public async Task<List<ListenHistoryDto>> GetListenHistoriesByTrackAsync(int trackId)
        {
            var histories = await _unitOfWork.ListenHistories.GetByTrackAsync(trackId);
            return _mapper.Map<List<ListenHistoryDto>>(histories);
        }

        public async Task AddListenHistoryAsync(ListenHistory listenHistory)
        {
            await _unitOfWork.ListenHistories.AddAsync(listenHistory);

            // Increment play count logic
            // This could be move to a more specialized method or handled automatically
            if (listenHistory.TrackId != null)
            {
                var track = await _unitOfWork.Tracks.GetByIdAsync(listenHistory.TrackId.Value);
                if (track != null)
                {
                    track.PlayCount++;
                    await _unitOfWork.Tracks.UpdateAsync(track);
                }
            }
            // Similar logic for other types... omitted for brevity

            await _unitOfWork.CompleteAsync();
        }

        public async Task<bool> HasListenedAsync(string userId, EntityType entityType, int entityId)
        {
            return await _unitOfWork.ListenHistories.HasListenedAsync(userId, entityType, entityId);
        }
    }
}
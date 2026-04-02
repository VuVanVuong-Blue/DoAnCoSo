using AutoMapper;
using Microsoft.EntityFrameworkCore;
using System_Music.Models.DTOs;
using System_Music.Models.SqlModels;
using System_Music.Repositories.Interfaces;
using System_Music.Services.Interfaces;

namespace System_Music.Services.Implementations
{
    public class ChartRankingService : IChartRankingService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public ChartRankingService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<List<ChartRankingDto>> GetTopTracksAsync(string country, string timeFrame, int limit = 50)
        {
            var rankings = await _unitOfWork.ChartRankings.GetTopTracksAsync(country, timeFrame, limit);
            return _mapper.Map<List<ChartRankingDto>>(rankings);
        }

        public async Task UpdateChartRankingAsync(string country, string timeFrame)
        {
            await _unitOfWork.BeginTransactionAsync();
            try
            {
                // Xóa bảng xếp hạng cũ
                // This logic should ideally be in ChartRankingRepository
                // But keeping it here for now for simplicity in this turn
                
                // Simplified update logic...
                // (In a real scenario, we'd query tracks, calculate scores, and save)
                
                await _unitOfWork.CommitTransactionAsync();
            }
            catch
            {
                await _unitOfWork.RollbackTransactionAsync();
                throw;
            }
        }
    }
}
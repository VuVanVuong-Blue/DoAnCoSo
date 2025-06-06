using System_Music.Models.SqlModels;
using System_Music.Repositories.Interfaces;
using System_Music.Services.Interfaces;

namespace System_Music.Services.Implementations
{
    public class ChartInteractionService : IChartInteractionService
    {
        private readonly IChartInteractionRepository _chartInteractionRepository;

        public ChartInteractionService(IChartInteractionRepository chartInteractionRepository)
        {
            _chartInteractionRepository = chartInteractionRepository;
        }

        public async Task<List<ChartInteraction>> GetAllInteractionsAsync()
        {
            return await _chartInteractionRepository.GetAllAsync();
        }

        public async Task<ChartInteraction> GetInteractionByIdAsync(int id)
        {
            return await _chartInteractionRepository.GetByIdAsync(id);
        }

        public async Task AddInteractionAsync(ChartInteraction interaction)
        {
            await _chartInteractionRepository.AddAsync(interaction);
        }

        public async Task DeleteInteractionAsync(int id)
        {
            await _chartInteractionRepository.DeleteAsync(id);
        }

        public async Task<List<ChartInteraction>> GetInteractionsByUserAsync(string userId)
        {
            return await _chartInteractionRepository.GetInteractionsByUserAsync(userId);
        }

        public async Task<List<ChartInteraction>> GetInteractionsByTrackAsync(int trackId)
        {
            return await _chartInteractionRepository.GetInteractionsByTrackAsync(trackId);
        }

        public async Task<int> GetInteractionCountByTrackAsync(int trackId)
        {
            var interactions = await _chartInteractionRepository.GetInteractionsByTrackAsync(trackId);
            return interactions.Count;
        }
    }
}
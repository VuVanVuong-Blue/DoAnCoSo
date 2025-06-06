using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System_Music.Services.Interfaces;

namespace System_Music.Services.Implementations
{
    public class UpdateChartRankingService : BackgroundService
    {
        private readonly IServiceProvider _services;

        public UpdateChartRankingService(IServiceProvider services)
        {
            _services = services;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                using (var scope = _services.CreateScope())
                {
                    var chartService = scope.ServiceProvider.GetRequiredService<IChartRankingService>();
                    await chartService.UpdateChartRankingAsync("Việt Nam", "daily");
                }
                // Chạy mỗi 24 giờ
                await Task.Delay(TimeSpan.FromHours(24), stoppingToken);
            }
        }
    }
}
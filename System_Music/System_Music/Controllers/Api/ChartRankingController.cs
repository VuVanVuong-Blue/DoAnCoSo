using Microsoft.AspNetCore.Mvc;
using System_Music.Models.SqlModels;
using System_Music.Services.Interfaces;

namespace System_Music.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ChartRankingController : ControllerBase
    {
        private readonly IChartRankingService _chartService;

        public ChartRankingController(IChartRankingService chartService)
        {
            _chartService = chartService;
        }

        [HttpGet("top")]
        public async Task<ActionResult<List<ChartRanking>>> GetTopTracks(string country = "Việt Nam", string timeFrame = "daily", int limit = 50)
        {
            var rankings = await _chartService.GetTopTracksAsync(country, timeFrame, limit);
            return Ok(rankings);
        }
    }
}
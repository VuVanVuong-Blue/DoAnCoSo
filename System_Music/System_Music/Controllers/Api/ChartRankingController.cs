using Microsoft.AspNetCore.Mvc;
using System_Music.Models.Common;
using System_Music.Models.DTOs;
using System_Music.Services.Interfaces;

namespace System_Music.Controllers.Api
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
        public async Task<IActionResult> GetTopTracks(string country = "Việt Nam", string timeFrame = "daily", int limit = 50)
        {
            var rankings = await _chartService.GetTopTracksAsync(country, timeFrame, limit);
            return Ok(ApiResult<List<ChartRankingDto>>.Success(rankings));
        }
    }
}
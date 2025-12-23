using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SkillSync.Api.Services.Analytics;

namespace SkillSync.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class AnalyticsController : ControllerBase
{
    private readonly IAnalyticsService _analyticsService;
    private readonly ILogger<AnalyticsController> _logger;

    public AnalyticsController(IAnalyticsService analyticsService, ILogger<AnalyticsController> logger)
    {
        _analyticsService = analyticsService;
        _logger = logger;
    }

    [HttpGet("overview")]
    public async Task<IActionResult> GetOverview()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        var stats = await _analyticsService.GetOverallStatsAsync(userId);
        return Ok(stats);
    }

    [HttpGet("progress-chart")]
    public async Task<IActionResult> GetProgressChart([FromQuery] int days = 30)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        var data = await _analyticsService.GetProgressChartDataAsync(userId, days);
        return Ok(data);
    }

    [HttpGet("skill-radar")]
    public async Task<IActionResult> GetSkillRadar()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        var data = await _analyticsService.GetSkillRadarDataAsync(userId);
        return Ok(data);
    }

    [HttpGet("activity-timeline")]
    public async Task<IActionResult> GetActivityTimeline([FromQuery] int days = 30)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        var data = await _analyticsService.GetActivityTimelineAsync(userId, days);
        return Ok(data);
    }
}
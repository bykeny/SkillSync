using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SkillSync.Api.DTOs.AI;
using SkillSync.Api.Services.AI;

namespace SkillSync.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class RecommendationsController : ControllerBase
{
    private readonly IAIRecommendationService _aiService;
    private readonly ILogger<RecommendationsController> _logger;

    public RecommendationsController(
        IAIRecommendationService aiService,
        ILogger<RecommendationsController> logger)
    {
        _aiService = aiService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> GetRecommendations()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        var recommendations = await _aiService.GetUserRecommendationsAsync(userId);
        return Ok(recommendations);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetRecommendation(int id)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        var recommendation = await _aiService.GetRecommendationByIdAsync(id, userId);

        if (recommendation == null)
            return NotFound(new { message = "Recommendation not found" });

        return Ok(recommendation);
    }

    [HttpPost("learning-path/{skillId}")]
    public async Task<IActionResult> GenerateLearningPath(int skillId)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        try
        {
            var result = await _aiService.GenerateLearningPathAsync(userId, skillId);
            return Ok(new { content = result });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("weekly-schedule")]
    public async Task<IActionResult> GenerateWeeklySchedule()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        var result = await _aiService.GenerateWeeklyScheduleAsync(userId);
        return Ok(new { content = result });
    }

    [HttpPost("skill-gap-analysis")]
    public async Task<IActionResult> GenerateSkillGapAnalysis()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        var result = await _aiService.GenerateSkillGapAnalysisAsync(userId);
        return Ok(new { content = result });
    }
}
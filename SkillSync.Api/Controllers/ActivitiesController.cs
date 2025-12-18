using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SkillSync.Api.DTOs.Activities;
using SkillSync.Api.Services.Activities;

namespace SkillSync.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ActivitiesController : ControllerBase
{
    private readonly IActivityService _activityService;
    private readonly ILogger<ActivitiesController> _logger;

    public ActivitiesController(IActivityService activityService, ILogger<ActivitiesController> logger)
    {
        _activityService = activityService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> GetAllActivities()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        var activities = await _activityService.GetAllActivitiesAsync(userId);
        return Ok(activities);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetActivity(int id)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        var activity = await _activityService.GetActivityByIdAsync(id, userId);

        if (activity == null)
            return NotFound(new { message = "Activity not found" });

        return Ok(activity);
    }

    [HttpPost]
    public async Task<IActionResult> CreateActivity([FromBody] CreateActivityDto dto)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        try
        {
            var activity = await _activityService.CreateActivityAsync(dto, userId);
            return CreatedAtAction(nameof(GetActivity), new { id = activity.Id }, activity);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateActivity(int id, [FromBody] UpdateActivityDto dto)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        try
        {
            var activity = await _activityService.UpdateActivityAsync(id, dto, userId);

            if (activity == null)
                return NotFound(new { message = "Activity not found" });

            return Ok(activity);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteActivity(int id)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        var result = await _activityService.DeleteActivityAsync(id, userId);

        if (!result)
            return NotFound(new { message = "Activity not found" });

        return Ok(new { message = "Activity deleted successfully" });
    }

    [HttpGet("skill/{skillId}")]
    public async Task<IActionResult> GetActivitiesBySkill(int skillId)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        var activities = await _activityService.GetActivitiesBySkillAsync(skillId, userId);
        return Ok(activities);
    }

    [HttpPost("{id}/start")]
    public async Task<IActionResult> StartActivity(int id)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        var activity = await _activityService.StartActivityAsync(id, userId);

        if (activity == null)
            return NotFound(new { message = "Activity not found" });

        return Ok(activity);
    }

    [HttpPost("{id}/complete")]
    public async Task<IActionResult> CompleteActivity(int id)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        var activity = await _activityService.CompleteActivityAsync(id, userId);

        if (activity == null)
            return NotFound(new { message = "Activity not found" });

        return Ok(activity);
    }

    [HttpGet("stats")]
    public async Task<IActionResult> GetActivityStats()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        var stats = await _activityService.GetActivityStatsAsync(userId);
        return Ok(stats);
    }
}
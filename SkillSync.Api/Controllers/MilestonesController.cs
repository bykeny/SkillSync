using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SkillSync.Api.DTOs.Milestones;
using SkillSync.Api.Services.Milestones;

namespace SkillSync.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class MilestonesController : ControllerBase
{
    private readonly IMilestoneService _milestoneService;
    private readonly ILogger<MilestonesController> _logger;

    public MilestonesController(IMilestoneService milestoneService, ILogger<MilestonesController> logger)
    {
        _milestoneService = milestoneService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> GetAllMilestones()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        var milestones = await _milestoneService.GetAllMilestonesAsync(userId);
        return Ok(milestones);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetMilestone(int id)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        var milestone = await _milestoneService.GetMilestoneByIdAsync(id, userId);

        if (milestone == null)
            return NotFound(new { message = "Milestone not found" });

        return Ok(milestone);
    }

    [HttpPost]
    public async Task<IActionResult> CreateMilestone([FromBody] CreateMilestoneDto dto)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        try
        {
            var milestone = await _milestoneService.CreateMilestoneAsync(dto, userId);
            return CreatedAtAction(nameof(GetMilestone), new { id = milestone.Id }, milestone);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateMilestone(int id, [FromBody] UpdateMilestoneDto dto)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var milestone = await _milestoneService.UpdateMilestoneAsync(id, dto, userId);

        if (milestone == null)
            return NotFound(new { message = "Milestone not found" });

        return Ok(milestone);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteMilestone(int id)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        var result = await _milestoneService.DeleteMilestoneAsync(id, userId);

        if (!result)
            return NotFound(new { message = "Milestone not found" });

        return Ok(new { message = "Milestone deleted successfully" });
    }

    [HttpGet("skill/{skillId}")]
    public async Task<IActionResult> GetMilestonesBySkill(int skillId)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        var milestones = await _milestoneService.GetMilestonesBySkillAsync(skillId, userId);
        return Ok(milestones);
    }

    [HttpPost("{id}/complete")]
    public async Task<IActionResult> CompleteMilestone(int id)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        var milestone = await _milestoneService.CompleteMilestoneAsync(id, userId);

        if (milestone == null)
            return NotFound(new { message = "Milestone not found" });

        return Ok(milestone);
    }

    [HttpGet("overdue")]
    public async Task<IActionResult> GetOverdueMilestones()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        var milestones = await _milestoneService.GetOverdueMilestonesAsync(userId);
        return Ok(milestones);
    }
}
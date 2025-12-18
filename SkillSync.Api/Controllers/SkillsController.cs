using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SkillSync.Api.DTOs.Skills;
using SkillSync.Api.Services.Skills;

namespace SkillSync.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class SkillsController : ControllerBase
{
    private readonly ISkillService _skillService;
    private readonly ILogger<SkillsController> _logger;

    public SkillsController(ISkillService skillService, ILogger<SkillsController> logger)
    {
        _skillService = skillService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> GetAllSkills()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        var skills = await _skillService.GetAllSkillsAsync(userId);
        return Ok(skills);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetSkill(int id)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        var skill = await _skillService.GetSkillByIdAsync(id, userId);

        if (skill == null)
            return NotFound(new { message = "Skill not found" });

        return Ok(skill);
    }

    [HttpPost]
    public async Task<IActionResult> CreateSkill([FromBody] CreateSkillDto dto)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var skill = await _skillService.CreateSkillAsync(dto, userId);

        return CreatedAtAction(
            nameof(GetSkill),
            new { id = skill.Id },
            skill
        );
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateSkill(int id, [FromBody] UpdateSkillDto dto)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var skill = await _skillService.UpdateSkillAsync(id, dto, userId);

        if (skill == null)
            return NotFound(new { message = "Skill not found" });

        return Ok(skill);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteSkill(int id)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        var result = await _skillService.DeleteSkillAsync(id, userId);

        if (!result)
            return NotFound(new { message = "Skill not found" });

        return Ok(new { message = "Skill deleted successfully" });
    }

    [HttpGet("categories")]
    [AllowAnonymous] // Categories can be public
    public async Task<IActionResult> GetCategories()
    {
        var categories = await _skillService.GetCategoriesAsync();
        return Ok(categories);
    }

    [HttpGet("category/{categoryId}")]
    public async Task<IActionResult> GetSkillsByCategory(int categoryId)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        var skills = await _skillService.GetSkillsByCategoryAsync(categoryId, userId);
        return Ok(skills);
    }
}
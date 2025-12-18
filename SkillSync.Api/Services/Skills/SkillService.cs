using Microsoft.EntityFrameworkCore;
using SkillSync.Api.Data;
using SkillSync.Api.Data.Entities;
using SkillSync.Api.DTOs.Skills;

namespace SkillSync.Api.Services.Skills;

public class SkillService : ISkillService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<SkillService> _logger;

    public SkillService(ApplicationDbContext context, ILogger<SkillService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<IEnumerable<SkillDto>> GetAllSkillsAsync(string userId)
    {
        var skills = await _context.Skills
            .Include(s => s.Category)
            .Where(s => s.UserId == userId)
            .OrderByDescending(s => s.CreatedAt)
            .ToListAsync();

        return skills.Select(MapToDto);
    }

    public async Task<SkillDto?> GetSkillByIdAsync(int id, string userId)
    {
        var skill = await _context.Skills
            .Include(s => s.Category)
            .FirstOrDefaultAsync(s => s.Id == id && s.UserId == userId);

        return skill == null ? null : MapToDto(skill);
    }

    public async Task<SkillDto> CreateSkillAsync(CreateSkillDto dto, string userId)
    {
        var skill = new Skill
        {
            Name = dto.Name,
            Description = dto.Description,
            ProficiencyLevel = dto.ProficiencyLevel,
            TargetLevel = dto.TargetLevel,
            CategoryId = dto.CategoryId,
            UserId = userId,
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        };

        _context.Skills.Add(skill);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Skill created: {SkillName} for user {UserId}", skill.Name, userId);

        // Reload with category
        await _context.Entry(skill).Reference(s => s.Category).LoadAsync();

        return MapToDto(skill);
    }

    public async Task<SkillDto?> UpdateSkillAsync(int id, UpdateSkillDto dto, string userId)
    {
        var skill = await _context.Skills
            .Include(s => s.Category)
            .FirstOrDefaultAsync(s => s.Id == id && s.UserId == userId);

        if (skill == null)
            return null;

        // Track progress history if proficiency level changed
        if (skill.ProficiencyLevel != dto.ProficiencyLevel)
        {
            var progressHistory = new ProgressHistory
            {
                SkillId = skill.Id,
                PreviousLevel = skill.ProficiencyLevel,
                NewLevel = dto.ProficiencyLevel,
                RecordedAt = DateTime.UtcNow
            };
            _context.ProgressHistories.Add(progressHistory);
        }

        skill.Name = dto.Name;
        skill.Description = dto.Description;
        skill.ProficiencyLevel = dto.ProficiencyLevel;
        skill.TargetLevel = dto.TargetLevel;
        skill.CategoryId = dto.CategoryId;
        skill.IsActive = dto.IsActive;
        skill.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        _logger.LogInformation("Skill updated: {SkillName} for user {UserId}", skill.Name, userId);

        return MapToDto(skill);
    }

    public async Task<bool> DeleteSkillAsync(int id, string userId)
    {
        var skill = await _context.Skills
            .FirstOrDefaultAsync(s => s.Id == id && s.UserId == userId);

        if (skill == null)
            return false;

        // Delete related activities first (due to Restrict delete behavior)
        var activities = await _context.Activities
            .Where(a => a.SkillId == id)
            .ToListAsync();

        _context.Activities.RemoveRange(activities);

        // Delete the skill
        _context.Skills.Remove(skill);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Skill deleted: {SkillName} for user {UserId}", skill.Name, userId);

        return true;
    }

    public async Task<IEnumerable<SkillCategoryDto>> GetCategoriesAsync()
    {
        var categories = await _context.SkillCategories
            .Select(c => new SkillCategoryDto
            {
                Id = c.Id,
                Name = c.Name,
                Description = c.Description,
                IconName = c.IconName,
                ColorHex = c.ColorHex,
                SkillCount = c.Skills.Count
            })
            .ToListAsync();

        return categories;
    }

    public async Task<IEnumerable<SkillDto>> GetSkillsByCategoryAsync(int categoryId, string userId)
    {
        var skills = await _context.Skills
            .Include(s => s.Category)
            .Where(s => s.CategoryId == categoryId && s.UserId == userId)
            .OrderByDescending(s => s.CreatedAt)
            .ToListAsync();

        return skills.Select(MapToDto);
    }

    private static SkillDto MapToDto(Skill skill)
    {
        return new SkillDto
        {
            Id = skill.Id,
            Name = skill.Name,
            Description = skill.Description,
            ProficiencyLevel = skill.ProficiencyLevel,
            TargetLevel = skill.TargetLevel,
            CategoryId = skill.CategoryId,
            CategoryName = skill.Category?.Name,
            CategoryColor = skill.Category?.ColorHex,
            CreatedAt = skill.CreatedAt,
            UpdatedAt = skill.UpdatedAt,
            IsActive = skill.IsActive
        };
    }
}
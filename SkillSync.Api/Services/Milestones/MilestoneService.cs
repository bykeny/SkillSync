using Microsoft.EntityFrameworkCore;
using SkillSync.Api.Data;
using SkillSync.Api.Data.Entities;
using SkillSync.Api.DTOs.Milestones;

namespace SkillSync.Api.Services.Milestones;

public class MilestoneService : IMilestoneService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<MilestoneService> _logger;

    public MilestoneService(ApplicationDbContext context, ILogger<MilestoneService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<IEnumerable<MilestoneDto>> GetAllMilestonesAsync(string userId)
    {
        var milestones = await _context.Milestones
            .Include(m => m.Skill)
            .Where(m => m.Skill.UserId == userId)
            .OrderBy(m => m.TargetDate)
            .ToListAsync();

        return milestones.Select(MapToDto);
    }

    public async Task<MilestoneDto?> GetMilestoneByIdAsync(int id, string userId)
    {
        var milestone = await _context.Milestones
            .Include(m => m.Skill)
            .FirstOrDefaultAsync(m => m.Id == id && m.Skill.UserId == userId);

        return milestone == null ? null : MapToDto(milestone);
    }

    public async Task<MilestoneDto> CreateMilestoneAsync(CreateMilestoneDto dto, string userId)
    {
        // Verify skill belongs to user
        var skill = await _context.Skills
            .FirstOrDefaultAsync(s => s.Id == dto.SkillId && s.UserId == userId);

        if (skill == null)
            throw new InvalidOperationException("Skill not found or does not belong to user");

        var milestone = new Milestone
        {
            Title = dto.Title,
            Description = dto.Description,
            TargetDate = dto.TargetDate,
            SkillId = dto.SkillId,
            CreatedAt = DateTime.UtcNow,
            IsCompleted = false,
            ProgressPercentage = 0
        };

        _context.Milestones.Add(milestone);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Milestone created: {Title} for skill {SkillId}", milestone.Title, dto.SkillId);

        // Reload with skill
        await _context.Entry(milestone).Reference(m => m.Skill).LoadAsync();

        return MapToDto(milestone);
    }

    public async Task<MilestoneDto?> UpdateMilestoneAsync(int id, UpdateMilestoneDto dto, string userId)
    {
        var milestone = await _context.Milestones
            .Include(m => m.Skill)
            .FirstOrDefaultAsync(m => m.Id == id && m.Skill.UserId == userId);

        if (milestone == null)
            return null;

        milestone.Title = dto.Title;
        milestone.Description = dto.Description;
        milestone.TargetDate = dto.TargetDate;
        milestone.ProgressPercentage = dto.ProgressPercentage;
        milestone.IsCompleted = dto.IsCompleted;
        milestone.UpdatedAt = DateTime.UtcNow;

        if (dto.IsCompleted && milestone.CompletedAt == null)
        {
            milestone.CompletedAt = DateTime.UtcNow;
            milestone.ProgressPercentage = 100;
        }

        await _context.SaveChangesAsync();

        _logger.LogInformation("Milestone updated: {Title}", milestone.Title);

        return MapToDto(milestone);
    }

    public async Task<bool> DeleteMilestoneAsync(int id, string userId)
    {
        var milestone = await _context.Milestones
            .Include(m => m.Skill)
            .FirstOrDefaultAsync(m => m.Id == id && m.Skill.UserId == userId);

        if (milestone == null)
            return false;

        _context.Milestones.Remove(milestone);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Milestone deleted: {Title}", milestone.Title);

        return true;
    }

    public async Task<IEnumerable<MilestoneDto>> GetMilestonesBySkillAsync(int skillId, string userId)
    {
        var milestones = await _context.Milestones
            .Include(m => m.Skill)
            .Where(m => m.SkillId == skillId && m.Skill.UserId == userId)
            .OrderBy(m => m.TargetDate)
            .ToListAsync();

        return milestones.Select(MapToDto);
    }

    public async Task<MilestoneDto?> CompleteMilestoneAsync(int id, string userId)
    {
        var milestone = await _context.Milestones
            .Include(m => m.Skill)
            .FirstOrDefaultAsync(m => m.Id == id && m.Skill.UserId == userId);

        if (milestone == null)
            return null;

        milestone.IsCompleted = true;
        milestone.CompletedAt = DateTime.UtcNow;
        milestone.ProgressPercentage = 100;
        milestone.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        _logger.LogInformation("Milestone completed: {Title}", milestone.Title);

        return MapToDto(milestone);
    }

    public async Task<IEnumerable<MilestoneDto>> GetOverdueMilestonesAsync(string userId)
    {
        var milestones = await _context.Milestones
            .Include(m => m.Skill)
            .Where(m => m.Skill.UserId == userId
                && !m.IsCompleted
                && m.TargetDate < DateTime.UtcNow)
            .OrderBy(m => m.TargetDate).ToListAsync();

        return milestones.Select(MapToDto);
    }

    private static MilestoneDto MapToDto(Milestone milestone)
    {
        return new MilestoneDto
        {
            Id = milestone.Id,
            Title = milestone.Title,
            Description = milestone.Description,
            TargetDate = milestone.TargetDate,
            CompletedAt = milestone.CompletedAt,
            IsCompleted = milestone.IsCompleted,
            ProgressPercentage = milestone.ProgressPercentage,
            CreatedAt = milestone.CreatedAt,
            SkillId = milestone.SkillId,
            SkillName = milestone.Skill?.Name ?? "Unknown"
        };
    }
}
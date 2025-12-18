using Microsoft.EntityFrameworkCore;
using SkillSync.Api.Data;
using SkillSync.Api.Data.Entities;
using SkillSync.Api.DTOs.Activities;

namespace SkillSync.Api.Services.Activities;

public class ActivityService : IActivityService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<ActivityService> _logger;

    public ActivityService(ApplicationDbContext context, ILogger<ActivityService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<IEnumerable<ActivityDto>> GetAllActivitiesAsync(string userId)
    {
        var activities = await _context.Activities
            .Include(a => a.Skill)
            .Where(a => a.UserId == userId)
            .OrderByDescending(a => a.CreatedAt)
            .ToListAsync();

        return activities.Select(MapToDto);
    }

    public async Task<ActivityDto?> GetActivityByIdAsync(int id, string userId)
    {
        var activity = await _context.Activities
            .Include(a => a.Skill)
            .FirstOrDefaultAsync(a => a.Id == id && a.UserId == userId);

        return activity == null ? null : MapToDto(activity);
    }

    public async Task<ActivityDto> CreateActivityAsync(CreateActivityDto dto, string userId)
    {
        // Verify skill belongs to user
        var skill = await _context.Skills
            .FirstOrDefaultAsync(s => s.Id == dto.SkillId && s.UserId == userId);

        if (skill == null)
            throw new InvalidOperationException("Skill not found or does not belong to user");

        var activity = new Activity
        {
            Title = dto.Title,
            Description = dto.Description,
            Type = dto.Type,
            Status = ActivityStatus.NotStarted,
            DurationMinutes = dto.DurationMinutes,
            ResourceUrl = dto.ResourceUrl,
            Notes = dto.Notes,
            SkillId = dto.SkillId,
            UserId = userId,
            CreatedAt = DateTime.UtcNow
        };

        _context.Activities.Add(activity);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Activity created: {Title} for skill {SkillId}", activity.Title, dto.SkillId);

        // Reload with skill
        await _context.Entry(activity).Reference(a => a.Skill).LoadAsync();

        return MapToDto(activity);
    }

    public async Task<ActivityDto?> UpdateActivityAsync(int id, UpdateActivityDto dto, string userId)
    {
        var activity = await _context.Activities
            .Include(a => a.Skill)
            .FirstOrDefaultAsync(a => a.Id == id && a.UserId == userId);

        if (activity == null)
            return null;

        // Verify new skill belongs to user if changed
        if (activity.SkillId != dto.SkillId)
        {
            var skill = await _context.Skills
                .FirstOrDefaultAsync(s => s.Id == dto.SkillId && s.UserId == userId);

            if (skill == null)
                throw new InvalidOperationException("Skill not found or does not belong to user");
        }

        var oldStatus = activity.Status;

        activity.Title = dto.Title;
        activity.Description = dto.Description;
        activity.Type = dto.Type;
        activity.Status = dto.Status;
        activity.DurationMinutes = dto.DurationMinutes;
        activity.ResourceUrl = dto.ResourceUrl;
        activity.Notes = dto.Notes;
        activity.SkillId = dto.SkillId;
        activity.UpdatedAt = DateTime.UtcNow;

        // Auto-set dates based on status changes
        if (oldStatus == ActivityStatus.NotStarted && dto.Status == ActivityStatus.InProgress)
        {
            activity.StartedAt = DateTime.UtcNow;
        }
        else if (dto.Status == ActivityStatus.Completed && activity.CompletedAt == null)
        {
            activity.CompletedAt = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync();

        _logger.LogInformation("Activity updated: {Title}", activity.Title);

        return MapToDto(activity);
    }

    public async Task<bool> DeleteActivityAsync(int id, string userId)
    {
        var activity = await _context.Activities
            .FirstOrDefaultAsync(a => a.Id == id && a.UserId == userId);

        if (activity == null)
            return false;

        _context.Activities.Remove(activity);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Activity deleted: {Title}", activity.Title);

        return true;
    }

    public async Task<IEnumerable<ActivityDto>> GetActivitiesBySkillAsync(int skillId, string userId)
    {
        var activities = await _context.Activities
            .Include(a => a.Skill)
            .Where(a => a.SkillId == skillId && a.UserId == userId)
            .OrderByDescending(a => a.CreatedAt)
            .ToListAsync();

        return activities.Select(MapToDto);
    }

    public async Task<ActivityDto?> StartActivityAsync(int id, string userId)
    {
        var activity = await _context.Activities
            .Include(a => a.Skill)
            .FirstOrDefaultAsync(a => a.Id == id && a.UserId == userId);

        if (activity == null)
            return null;

        activity.Status = ActivityStatus.InProgress;
        activity.StartedAt = DateTime.UtcNow;
        activity.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        _logger.LogInformation("Activity started: {Title}", activity.Title);

        return MapToDto(activity);
    }

    public async Task<ActivityDto?> CompleteActivityAsync(int id, string userId)
    {
        var activity = await _context.Activities
            .Include(a => a.Skill)
            .FirstOrDefaultAsync(a => a.Id == id && a.UserId == userId);

        if (activity == null)
            return null;

        activity.Status = ActivityStatus.Completed;
        activity.CompletedAt = DateTime.UtcNow;
        activity.UpdatedAt = DateTime.UtcNow;

        if (activity.StartedAt == null)
            activity.StartedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        _logger.LogInformation("Activity completed: {Title}", activity.Title);

        return MapToDto(activity);
    }

    public async Task<ActivityStatsDto> GetActivityStatsAsync(string userId)
    {
        var activities = await _context.Activities
            .Where(a => a.UserId == userId)
            .ToListAsync();

        var weekStart = DateTime.UtcNow.AddDays(-7);
        var thisWeekActivities = activities.Where(a => a.CreatedAt >= weekStart);

        return new ActivityStatsDto
        {
            TotalActivities = activities.Count,
            CompletedActivities = activities.Count(a => a.Status == ActivityStatus.Completed),
            InProgressActivities = activities.Count(a => a.Status == ActivityStatus.InProgress),
            TotalMinutes = activities.Sum(a => a.DurationMinutes),
            ThisWeekMinutes = thisWeekActivities.Sum(a => a.DurationMinutes)
        };
    }

    private static ActivityDto MapToDto(Activity activity)
    {
        return new ActivityDto
        {
            Id = activity.Id,
            Title = activity.Title,
            Description = activity.Description,
            Type = activity.Type,
            Status = activity.Status,
            DurationMinutes = activity.DurationMinutes,
            StartedAt = activity.StartedAt,
            CompletedAt = activity.CompletedAt,
            CreatedAt = activity.CreatedAt,
            ResourceUrl = activity.ResourceUrl,
            Notes = activity.Notes,
            SkillId = activity.SkillId,
            SkillName = activity.Skill?.Name ?? "Unknown"
        };
    }
}
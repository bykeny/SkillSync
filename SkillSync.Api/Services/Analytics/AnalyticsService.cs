using Microsoft.EntityFrameworkCore;
using SkillSync.Api.Data;
using SkillSync.Api.Data.Entities;
using SkillSync.Api.DTOs.Analytics;

namespace SkillSync.Api.Services.Analytics;

public class AnalyticsService : IAnalyticsService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<AnalyticsService> _logger;

    public AnalyticsService(ApplicationDbContext context, ILogger<AnalyticsService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<OverallStatsDto> GetOverallStatsAsync(string userId)
    {
        var skills = await _context.Skills
            .Include(s => s.Category)
            .Where(s => s.UserId == userId)
            .ToListAsync();

        var activities = await _context.Activities
            .Where(a => a.UserId == userId)
            .ToListAsync();

        var weekStart = DateTime.UtcNow.AddDays(-7);
        var monthStart = DateTime.UtcNow.AddDays(-30);

        var categoryStats = skills
            .Where(s => s.Category != null)
            .GroupBy(s => s.Category!.Name)
            .Select(g => new CategoryStatsDto
            {
                CategoryName = g.Key,
                SkillCount = g.Count(),
                AverageProficiency = g.Average(s => s.ProficiencyLevel)
            })
            .ToList();

        return new OverallStatsDto
        {
            TotalSkills = skills.Count,
            ActiveSkills = skills.Count(s => s.IsActive),
            AverageProficiency = skills.Any() ? skills.Average(s => s.ProficiencyLevel) : 0,
            TotalActivities = activities.Count,
            CompletedActivities = activities.Count(a => a.Status == ActivityStatus.Completed),
            TotalLearningHours = activities.Sum(a => a.DurationMinutes) / 60,
            CurrentWeekHours = activities.Where(a => a.CreatedAt >= weekStart).Sum(a => a.DurationMinutes) / 60,
            CurrentMonthHours = activities.Where(a => a.CreatedAt >= monthStart).Sum(a => a.DurationMinutes) / 60,
            CategoryStats = categoryStats
        };
    }

    public async Task<ProgressChartDto> GetProgressChartDataAsync(string userId, int days = 30)
    {
        var startDate = DateTime.UtcNow.AddDays(-days);

        var progressHistory = await _context.ProgressHistories
            .Include(ph => ph.Skill)
            .Where(ph => ph.Skill.UserId == userId && ph.RecordedAt >= startDate)
            .OrderBy(ph => ph.RecordedAt)
            .ToListAsync();

        var skills = await _context.Skills
            .Where(s => s.UserId == userId && s.IsActive)
            .OrderBy(s => s.Name)
            .Take(5) // Top 5 skills for clarity
            .ToListAsync();

        var result = new ProgressChartDto();

        // Generate date labels
        for (int i = days; i >= 0; i--)
        {
            var date = DateTime.UtcNow.AddDays(-i).Date;
            result.Labels.Add(date.ToString("MMM dd"));
        }

        // Generate series data for each skill
        foreach (var skill in skills)
        {
            var levels = new List<int>();
            var currentLevel = skill.ProficiencyLevel;

            for (int i = days; i >= 0; i--)
            {
                var date = DateTime.UtcNow.AddDays(-i).Date;

                // Check if there's a progress record for this date
                var progress = progressHistory
                    .Where(ph => ph.SkillId == skill.Id && ph.RecordedAt.Date == date)
                    .OrderByDescending(ph => ph.RecordedAt)
                    .FirstOrDefault();

                if (progress != null)
                {
                    currentLevel = progress.NewLevel;
                }

                levels.Add(currentLevel);
            }

            result.Series[skill.Name] = levels;
        }

        return result;
    }

    public async Task<SkillRadarDto> GetSkillRadarDataAsync(string userId)
    {
        var skills = await _context.Skills
            .Where(s => s.UserId == userId && s.IsActive)
            .OrderByDescending(s => s.ProficiencyLevel)
            .Take(8) // Top 8 skills for radar chart
            .ToListAsync();

        return new SkillRadarDto
        {
            Labels = skills.Select(s => s.Name).ToList(),
            CurrentLevels = skills.Select(s => s.ProficiencyLevel).ToList(),
            TargetLevels = skills.Select(s => s.TargetLevel).ToList()
        };
    }

    public async Task<ActivityTimelineDto> GetActivityTimelineAsync(string userId, int days = 30)
    {
        var startDate = DateTime.UtcNow.AddDays(-days);

        var activities = await _context.Activities
            .Where(a => a.UserId == userId && a.CreatedAt >= startDate)
            .ToListAsync();

        var timeline = activities
            .GroupBy(a => a.CreatedAt.Date)
            .Select(g => new ActivityDateDto
            {
                Date = g.Key,
                Count = g.Count(),
                TotalMinutes = g.Sum(a => a.DurationMinutes)
            })
            .OrderBy(a => a.Date)
            .ToList();

        return new ActivityTimelineDto { Activities = timeline };
    }
}
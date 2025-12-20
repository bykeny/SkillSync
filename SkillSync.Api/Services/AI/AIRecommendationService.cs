using System.Text;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using OpenAI.Chat;
using SkillSync.Api.Data;
using SkillSync.Api.Data.Entities;
using SkillSync.Api.DTOs.AI;

namespace SkillSync.Api.Services.AI;

public class AIRecommendationService : IAIRecommendationService
{
    private readonly ApplicationDbContext _context;
    private readonly OpenAISettings _settings;
    private readonly ILogger<AIRecommendationService> _logger;
    private readonly ChatClient _chatClient;

    public AIRecommendationService(
        ApplicationDbContext context,
        IOptions<OpenAISettings> settings,
        ILogger<AIRecommendationService> logger)
    {
        _context = context;
        _settings = settings.Value;
        _logger = logger;

        if (string.IsNullOrWhiteSpace(_settings.ApiKey))
        {
            throw new InvalidOperationException(
                "OpenAI API key is not configured. Please set 'OpenAI:ApiKey' in your configuration " +
                "(user secrets, environment variables, or appsettings.json).");
        }

        _chatClient = new ChatClient(_settings.Model, _settings.ApiKey);
    }

    public async Task<string> GenerateLearningPathAsync(string userId, int skillId)
    {
        var skill = await _context.Skills
            .Include(s => s.Category)
            .Include(s => s.Activities)
            .Include(s => s.Milestones)
            .FirstOrDefaultAsync(s => s.Id == skillId && s.UserId == userId);

        if (skill == null)
            throw new InvalidOperationException("Skill not found");

        var prompt = BuildLearningPathPrompt(skill);
        var response = await CallOpenAIAsync(prompt);

        // Save recommendation
        var recommendation = new AIRecommendation
        {
            UserId = userId,
            Title = $"Learning Path: {skill.Name}",
            Content = response,
            Type = RecommendationType.LearningPath,
            GeneratedAt = DateTime.UtcNow,
            IsActive = true,
            RelatedSkillIds = skillId.ToString()
        };

        _context.AIRecommendations.Add(recommendation);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Generated learning path for skill {SkillId}", skillId);

        return response;
    }

    public async Task<string> GenerateWeeklyScheduleAsync(string userId)
    {
        var skills = await _context.Skills
            .Include(s => s.Activities)
            .Where(s => s.UserId == userId && s.IsActive)
            .ToListAsync();

        if (!skills.Any())
            return "You don't have any active skills yet. Add some skills to get a personalized schedule!";

        var prompt = BuildWeeklySchedulePrompt(skills);
        var response = await CallOpenAIAsync(prompt);

        // Save recommendation
        var recommendation = new AIRecommendation
        {
            UserId = userId,
            Title = "Weekly Study Schedule",
            Content = response,
            Type = RecommendationType.WeeklySchedule,
            GeneratedAt = DateTime.UtcNow,
            IsActive = true,
            RelatedSkillIds = string.Join(",", skills.Select(s => s.Id))
        };

        _context.AIRecommendations.Add(recommendation);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Generated weekly schedule for user {UserId}", userId);

        return response;
    }

    public async Task<string> GenerateSkillGapAnalysisAsync(string userId)
    {
        var skills = await _context.Skills
            .Include(s => s.Category)
            .Where(s => s.UserId == userId && s.IsActive)
            .ToListAsync();

        if (!skills.Any())
            return "You don't have any skills tracked yet. Add some skills to get a gap analysis!";

        var prompt = BuildSkillGapPrompt(skills);
        var response = await CallOpenAIAsync(prompt);

        // Save recommendation
        var recommendation = new AIRecommendation
        {
            UserId = userId,
            Title = "Skill Gap Analysis",
            Content = response,
            Type = RecommendationType.SkillGapAnalysis,
            GeneratedAt = DateTime.UtcNow,
            IsActive = true
        };

        _context.AIRecommendations.Add(recommendation);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Generated skill gap analysis for user {UserId}", userId);

        return response;
    }

    public async Task<List<RecommendationDto>> GetUserRecommendationsAsync(string userId)
    {
        var recommendations = await _context.AIRecommendations
            .Where(r => r.UserId == userId && r.IsActive)
            .OrderByDescending(r => r.GeneratedAt)
            .Take(20)
            .ToListAsync();

        return recommendations.Select(r => new RecommendationDto
        {
            Id = r.Id,
            Title = r.Title,
            Content = r.Content,
            Type = r.Type.ToString(),
            GeneratedAt = r.GeneratedAt,
            IsActive = r.IsActive
        }).ToList();
    }

    public async Task<RecommendationDto?> GetRecommendationByIdAsync(int id, string userId)
    {
        var recommendation = await _context.AIRecommendations
            .FirstOrDefaultAsync(r => r.Id == id && r.UserId == userId);

        if (recommendation == null)
            return null;

        return new RecommendationDto
        {
            Id = recommendation.Id,
            Title = recommendation.Title,
            Content = recommendation.Content,
            Type = recommendation.Type.ToString(),
            GeneratedAt = recommendation.GeneratedAt,
            IsActive = recommendation.IsActive
        };
    }

    private async Task<string> CallOpenAIAsync(string prompt)
    {
        try
        {
            var messages = new List<ChatMessage>
            {
                new SystemChatMessage("You are a helpful learning advisor and skill development coach. Provide practical, actionable advice for developers and professionals looking to improve their skills. Format your responses in clear markdown with headers, bullet points, and emphasis where appropriate."),
                new UserChatMessage(prompt)
            };

            var completion = await _chatClient.CompleteChatAsync(messages);
            return completion.Value.Content[0].Text;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calling OpenAI API");
            throw new InvalidOperationException("Failed to generate AI recommendation. Please try again.", ex);
        }
    }

    private string BuildLearningPathPrompt(Skill skill)
    {
        var sb = new StringBuilder();
        sb.AppendLine($"Create a personalized learning path for improving the following skill:");
        sb.AppendLine();
        sb.AppendLine($"**Skill:** {skill.Name}");
        sb.AppendLine($"**Category:** {skill.Category?.Name ?? "General"}");
        sb.AppendLine($"**Current Level:** {skill.ProficiencyLevel}/5");
        sb.AppendLine($"**Target Level:** {skill.TargetLevel}/5");

        if (!string.IsNullOrEmpty(skill.Description))
            sb.AppendLine($"**Context:** {skill.Description}");

        sb.AppendLine();
        sb.AppendLine($"**Recent Activities:** {skill.Activities.Count} activities logged");

        if (skill.Activities.Any())
        {
            var completedCount = skill.Activities.Count(a => a.Status == ActivityStatus.Completed);
            sb.AppendLine($"- Completed: {completedCount}");
            sb.AppendLine($"- Total time spent: {skill.Activities.Sum(a => a.DurationMinutes) / 60.0:F1} hours");
        }

        sb.AppendLine();
        sb.AppendLine("Please provide:");
        sb.AppendLine("1. A step-by-step learning path from current to target level");
        sb.AppendLine("2. Specific topics to focus on at each stage");
        sb.AppendLine("3. Recommended resources (courses, books, projects)");
        sb.AppendLine("4. Estimated timeline for each stage");
        sb.AppendLine("5. Practical projects or exercises to solidify knowledge");

        return sb.ToString();
    }

    private string BuildWeeklySchedulePrompt(List<Skill> skills)
    {
        var sb = new StringBuilder();
        sb.AppendLine("Create a balanced weekly study schedule based on these skills:");
        sb.AppendLine();

        foreach (var skill in skills.OrderByDescending(s => s.TargetLevel - s.ProficiencyLevel).Take(5))
        {
            sb.AppendLine($"- **{skill.Name}** (Level {skill.ProficiencyLevel}→{skill.TargetLevel})");
            if (!string.IsNullOrEmpty(skill.Category?.Name))
                sb.AppendLine($"  Category: {skill.Category.Name}");
        }

        sb.AppendLine();
        sb.AppendLine("Please create a weekly schedule that:");
        sb.AppendLine("1. Allocates 7-10 hours total across the week");
        sb.AppendLine("2. Prioritizes skills with the biggest gaps");
        sb.AppendLine("3. Includes a mix of learning, practice, and projects");
        sb.AppendLine("4. Considers typical work-life balance (weekday evenings + weekends)");
        sb.AppendLine("5. Includes specific, actionable tasks for each session");
        sb.AppendLine();
        sb.AppendLine("Format as a day-by-day schedule with time blocks.");

        return sb.ToString();
    }

    private string BuildSkillGapPrompt(List<Skill> skills)
    {
        var sb = new StringBuilder();
        sb.AppendLine("Analyze the skill gaps in this developer's profile:");
        sb.AppendLine();

        var grouped = skills.GroupBy(s => s.Category?.Name ?? "Other");

        foreach (var group in grouped)
        {
            sb.AppendLine($"**{group.Key}:**");
            foreach (var skill in group)
            {
                var gap = skill.TargetLevel - skill.ProficiencyLevel;
                sb.AppendLine($"- {skill.Name}: Level {skill.ProficiencyLevel}/5 (Gap: {gap})");
            }
            sb.AppendLine();
        }

        sb.AppendLine("Please provide:");
        sb.AppendLine("1. Analysis of current skill distribution and balance");
        sb.AppendLine("2. Identified gaps and weaknesses");
        sb.AppendLine("3. Complementary skills that would enhance the portfolio");
        sb.AppendLine("4. Priority recommendations for the next 3 months");
        sb.AppendLine("5. Career path suggestions based on current skills");

        return sb.ToString();
    }
}
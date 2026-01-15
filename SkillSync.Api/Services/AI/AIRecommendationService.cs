using System.Text;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Mscc.GenerativeAI;
using SkillSync.Api.Data;
using SkillSync.Api.Data.Entities;
using SkillSync.Api.DTOs.AI;

namespace SkillSync.Api.Services.AI;

public class AIRecommendationService : IAIRecommendationService
{
    private readonly ApplicationDbContext _context;
    private readonly GeminiSettings _settings;
    private readonly ILogger<AIRecommendationService> _logger;
    private readonly IRateLimitService _rateLimitService;
    private readonly GoogleAI _geminiClient;

    public AIRecommendationService(
        ApplicationDbContext context,
        IOptions<GeminiSettings> settings,
        ILogger<AIRecommendationService> logger,
        IRateLimitService rateLimitService)
    {
        _context = context;
        _settings = settings.Value;
        _logger = logger;
        _rateLimitService = rateLimitService;

        if (string.IsNullOrWhiteSpace(_settings.ApiKey))
        {
            throw new InvalidOperationException(
                "Gemini API key is not configured. Please set 'Gemini:ApiKey' in your configuration " +
                "(user secrets, environment variables, or appsettings.json).");
        }

        _geminiClient = new GoogleAI(_settings.ApiKey);
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
        var response = await CallGeminiAsync(prompt);

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
        var response = await CallGeminiAsync(prompt);

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
        var response = await CallGeminiAsync(prompt);

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

    private async Task<string> CallGeminiAsync(string prompt)
    {
        try
        {
            // Wait for rate limit before making the call
            await _rateLimitService.WaitForRateLimitAsync();
            
            var model = _geminiClient.GenerativeModel(_settings.Model);
            
            // Optimize system instruction for Gemini
            var systemInstruction = @"You are an expert learning advisor and skill development coach specializing in helping developers and professionals grow their careers. Your advice should be:
- Practical and immediately actionable
- Tailored to the user's current situation
- Structured with clear headers and bullet points
- Focused on modern best practices and industry standards
- Encouraging yet realistic about timelines and effort required

Format all responses in clear markdown with headers (##, ###), bullet points, and **bold** emphasis where appropriate.";

            var fullPrompt = $"{systemInstruction}\n\n{prompt}";
            
            var response = await model.GenerateContent(fullPrompt);
            
            // Record the successful API call
            _rateLimitService.RecordApiCall();
            
            return response.Text ?? throw new InvalidOperationException("Gemini returned an empty response");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calling Gemini API");
            throw new InvalidOperationException("Failed to generate AI recommendation. Please try again.", ex);
        }
    }

    private string BuildLearningPathPrompt(Skill skill)
    {
        var sb = new StringBuilder();
        sb.AppendLine($"# Task: Create a Personalized Learning Path");
        sb.AppendLine();
        sb.AppendLine($"## Skill Information:");
        sb.AppendLine($"- **Skill Name:** {skill.Name}");
        sb.AppendLine($"- **Category:** {skill.Category?.Name ?? "General"}");
        sb.AppendLine($"- **Current Proficiency:** {skill.ProficiencyLevel}/5");
        sb.AppendLine($"- **Target Proficiency:** {skill.TargetLevel}/5");
        sb.AppendLine($"- **Proficiency Gap:** {skill.TargetLevel - skill.ProficiencyLevel} levels to improve");

        if (!string.IsNullOrEmpty(skill.Description))
            sb.AppendLine($"- **Additional Context:** {skill.Description}");

        sb.AppendLine();
        sb.AppendLine($"## Progress Tracking:");
        sb.AppendLine($"- Total activities logged: {skill.Activities.Count}");

        if (skill.Activities.Any())
        {
            var completedCount = skill.Activities.Count(a => a.Status == ActivityStatus.Completed);
            var totalHours = skill.Activities.Sum(a => a.DurationMinutes) / 60.0;
            sb.AppendLine($"- Completed activities: {completedCount}");
            sb.AppendLine($"- Total study time invested: {totalHours:F1} hours");
        }

        sb.AppendLine();
        sb.AppendLine("## Required Output:");
        sb.AppendLine("Create a comprehensive, actionable learning path structured as follows:");
        sb.AppendLine();
        sb.AppendLine("### 1. Learning Roadmap");
        sb.AppendLine("Break down the journey from current level to target level into clear stages.");
        sb.AppendLine();
        sb.AppendLine("### 2. Topic Breakdown");
        sb.AppendLine("For each stage, list specific topics, concepts, and technologies to master.");
        sb.AppendLine();
        sb.AppendLine("### 3. Recommended Resources");
        sb.AppendLine("Suggest high-quality courses, books, documentation, tutorials, and video content for each stage.");
        sb.AppendLine();
        sb.AppendLine("### 4. Practical Projects");
        sb.AppendLine("Propose hands-on projects that will solidify learning and build portfolio pieces.");
        sb.AppendLine();
        sb.AppendLine("### 5. Timeline Estimation");
        sb.AppendLine("Provide realistic time estimates for each stage based on 5-10 hours per week of dedicated study.");
        sb.AppendLine();
        sb.AppendLine("### 6. Milestones & Success Criteria");
        sb.AppendLine("Define clear checkpoints to measure progress at each stage.");

        return sb.ToString();
    }

    private string BuildWeeklySchedulePrompt(List<Skill> skills)
    {
        var sb = new StringBuilder();
        sb.AppendLine("# Task: Design a Balanced Weekly Study Schedule");
        sb.AppendLine();
        sb.AppendLine("## Active Skills Requiring Development:");

        var prioritizedSkills = skills
            .OrderByDescending(s => s.TargetLevel - s.ProficiencyLevel)
            .ThenByDescending(s => s.TargetLevel)
            .Take(5)
            .ToList();

        foreach (var skill in prioritizedSkills)
        {
            var gap = skill.TargetLevel - skill.ProficiencyLevel;
            sb.AppendLine($"- **{skill.Name}** ({skill.Category?.Name ?? "General"})");
            sb.AppendLine($"  - Current Level: {skill.ProficiencyLevel}/5");
            sb.AppendLine($"  - Target Level: {skill.TargetLevel}/5");
            sb.AppendLine($"  - Gap: {gap} levels");
        }

        sb.AppendLine();
        sb.AppendLine("## Schedule Requirements:");
        sb.AppendLine("- **Total Weekly Hours:** 7-10 hours distributed across the week");
        sb.AppendLine("- **Priority System:** Allocate more time to skills with larger proficiency gaps");
        sb.AppendLine("- **Learning Balance:** Mix of theory, practice, and project work");
        sb.AppendLine("- **Work-Life Balance:** Consider typical work schedules (evenings 7-9 PM on weekdays, flexible weekend blocks)");
        sb.AppendLine();
        sb.AppendLine("## Required Output:");
        sb.AppendLine("Create a day-by-day schedule with the following structure:");
        sb.AppendLine();
        sb.AppendLine("### For each day (Monday-Sunday):");
        sb.AppendLine("- Time slot (e.g., 7:00 PM - 8:30 PM)");
        sb.AppendLine("- Skill to focus on");
        sb.AppendLine("- Specific task or activity (e.g., 'Complete React hooks tutorial', 'Build mini-project')");
        sb.AppendLine("- Duration in minutes");
        sb.AppendLine();
        sb.AppendLine("### Additional Elements:");
        sb.AppendLine("- Include at least one rest day");
        sb.AppendLine("- Suggest short 15-minute review sessions for reinforcement");
        sb.AppendLine("- Recommend one longer weekend session (2-3 hours) for project work");

        return sb.ToString();
    }

    private string BuildSkillGapPrompt(List<Skill> skills)
    {
        var sb = new StringBuilder();
        sb.AppendLine("# Task: Comprehensive Skill Gap Analysis");
        sb.AppendLine();
        sb.AppendLine("## Current Skill Portfolio:");

        var grouped = skills.GroupBy(s => s.Category?.Name ?? "Uncategorized")
            .OrderByDescending(g => g.Count());

        foreach (var group in grouped)
        {
            sb.AppendLine();
            sb.AppendLine($"### {group.Key}:");
            foreach (var skill in group.OrderByDescending(s => s.ProficiencyLevel))
            {
                var gap = skill.TargetLevel - skill.ProficiencyLevel;
                var gapIndicator = gap > 2 ? "⚠️ Large Gap" : gap > 0 ? "→ Growing" : "✓ Target Reached";
                sb.AppendLine($"- **{skill.Name}**: Level {skill.ProficiencyLevel}/5 → Target: {skill.TargetLevel}/5 ({gapIndicator})");
            }
        }

        sb.AppendLine();
        sb.AppendLine("## Analysis Requirements:");
        sb.AppendLine();
        sb.AppendLine("### 1. Skill Distribution Assessment");
        sb.AppendLine("Analyze the balance across different categories. Identify if the developer is:");
        sb.AppendLine("- Well-rounded or specialized");
        sb.AppendLine("- Frontend-heavy, backend-heavy, or full-stack");
        sb.AppendLine("- Lacking in any critical areas");
        sb.AppendLine();
        sb.AppendLine("### 2. Critical Gaps Identification");
        sb.AppendLine("Highlight the most significant gaps that should be prioritized, considering:");
        sb.AppendLine("- Skills with largest proficiency gaps");
        sb.AppendLine("- Industry demands and market trends");
        sb.AppendLine("- Synergies between existing and missing skills");
        sb.AppendLine();
        sb.AppendLine("### 3. Complementary Skills Recommendations");
        sb.AppendLine("Suggest 3-5 new skills that would:");
        sb.AppendLine("- Enhance the existing skill set");
        sb.AppendLine("- Fill obvious gaps in the portfolio");
        sb.AppendLine("- Increase marketability and career opportunities");
        sb.AppendLine();
        sb.AppendLine("### 4. 90-Day Priority Action Plan");
        sb.AppendLine("Recommend specific skills to focus on for the next 3 months, with clear reasoning.");
        sb.AppendLine();
        sb.AppendLine("### 5. Career Path Alignment");
        sb.AppendLine("Based on the current skills, suggest 2-3 career paths or roles that would be a good fit:");
        sb.AppendLine("- Roles that align well with current strengths");
        sb.AppendLine("- Emerging opportunities that match the skill trajectory");
        sb.AppendLine("- What additional skills would be needed for each path");

        return sb.ToString();
    }
}
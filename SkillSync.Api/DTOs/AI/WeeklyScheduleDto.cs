namespace SkillSync.Api.DTOs.AI;

public class WeeklyScheduleDto
{
    public Dictionary<string, List<ScheduleItem>> Schedule { get; set; } = new();
}

public class ScheduleItem
{
    public string Time { get; set; } = string.Empty;
    public string Activity { get; set; } = string.Empty;
    public string Skill { get; set; } = string.Empty;
    public int DurationMinutes { get; set; }
}
namespace SkillSync.Api.DTOs.Analytics;

public class ActivityTimelineDto
{
    public List<ActivityDateDto> Activities { get; set; } = new();
}

public class ActivityDateDto
{
    public DateTime Date { get; set; }
    public int Count { get; set; }
    public int TotalMinutes { get; set; }
}
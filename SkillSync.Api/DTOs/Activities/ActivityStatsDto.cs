namespace SkillSync.Api.DTOs.Activities;

public class ActivityStatsDto
{
    public int TotalActivities { get; set; }
    public int CompletedActivities { get; set; }
    public int InProgressActivities { get; set; }
    public int TotalMinutes { get; set; }
    public double TotalHours => TotalMinutes / 60.0;
    public int ThisWeekMinutes { get; set; }
    public double ThisWeekHours => ThisWeekMinutes / 60.0;
}
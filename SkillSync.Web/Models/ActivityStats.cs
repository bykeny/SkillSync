namespace SkillSync.Web.Models;

public class ActivityStats
{
    public int TotalActivities { get; set; }
    public int CompletedActivities { get; set; }
    public int InProgressActivities { get; set; }
    public int TotalMinutes { get; set; }
    public double TotalHours { get; set; }
    public int ThisWeekMinutes { get; set; }
    public double ThisWeekHours { get; set; }
}
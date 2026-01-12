namespace SkillSync.Web.Models;

public class ActivityTimelineData
{
    public List<ActivityDateData> Activities { get; set; } = new();
}

public class ActivityDateData
{
    public DateTime Date { get; set; }
    public int Count { get; set; }
    public int TotalMinutes { get; set; }
}

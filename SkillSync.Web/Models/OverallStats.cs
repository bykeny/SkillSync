namespace SkillSync.Web.Models;

public class OverallStats
{
    public int TotalSkills { get; set; }
    public int ActiveSkills { get; set; }
    public double AverageProficiency { get; set; }
    public int TotalActivities { get; set; }
    public int CompletedActivities { get; set; }
    public int TotalLearningHours { get; set; }
    public int CurrentWeekHours { get; set; }
    public int CurrentMonthHours { get; set; }
    public List<CategoryStats> CategoryStats { get; set; } = new();
}

public class CategoryStats
{
    public string CategoryName { get; set; } = string.Empty;
    public int SkillCount { get; set; }
    public double AverageProficiency { get; set; }
}
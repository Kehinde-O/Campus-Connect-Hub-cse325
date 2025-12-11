namespace CampusConnectHub.Shared.DTOs;

public class AnalyticsDto
{
    public int TotalUsers { get; set; }
    public int NewUsersThisMonth { get; set; }
    public int TotalNewsPosts { get; set; }
    public int PublishedNewsPosts { get; set; }
    public int TotalEvents { get; set; }
    public int UpcomingEvents { get; set; }
    public int TotalRSVPs { get; set; }
    public int TotalResources { get; set; }
    public double AverageRSVPsPerEvent { get; set; }
    public List<MonthlyTrend> UserGrowthTrend { get; set; } = new();
    public List<MonthlyTrend> EventTrend { get; set; } = new();
    public List<MonthlyTrend> RSVPTrend { get; set; } = new();
    public List<EventPopularityDto> MostPopularEvents { get; set; } = new();
}

public class MonthlyTrend
{
    public string Month { get; set; } = string.Empty;
    public int Count { get; set; }
}

public class EventPopularityDto
{
    public int EventId { get; set; }
    public string EventTitle { get; set; } = string.Empty;
    public DateTime EventDate { get; set; }
    public int RSVPCount { get; set; }
}


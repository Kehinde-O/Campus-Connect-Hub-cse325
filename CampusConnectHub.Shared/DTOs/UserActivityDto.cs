namespace CampusConnectHub.Shared.DTOs;

public class UserActivityDto
{
    public int UserId { get; set; }
    public int TotalNewsPosts { get; set; }
    public int TotalEventsCreated { get; set; }
    public int TotalRSVPs { get; set; }
    public DateTime LastActivityDate { get; set; }
}


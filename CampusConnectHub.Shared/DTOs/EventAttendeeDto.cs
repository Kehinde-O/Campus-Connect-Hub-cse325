namespace CampusConnectHub.Shared.DTOs;

public class EventAttendeeDto
{
    public int RSVPId { get; set; }
    public int UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string UserEmail { get; set; } = string.Empty;
    public DateTime RSVPDate { get; set; }
    public string Status { get; set; } = string.Empty;
}


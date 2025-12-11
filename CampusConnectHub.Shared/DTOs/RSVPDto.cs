namespace CampusConnectHub.Shared.DTOs;

public class RSVPDto
{
    public int Id { get; set; }
    public int EventId { get; set; }
    public string EventTitle { get; set; } = string.Empty;
    public DateTime EventDate { get; set; }
    public int UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public DateTime RSVPDate { get; set; }
    public string Status { get; set; } = "Confirmed";
}


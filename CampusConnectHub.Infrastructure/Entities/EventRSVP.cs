namespace CampusConnectHub.Infrastructure.Entities;

public class EventRSVP
{
    public int Id { get; set; }
    public int EventId { get; set; }
    public int UserId { get; set; }
    public DateTime RSVPDate { get; set; } = DateTime.UtcNow;
    public string Status { get; set; } = "Confirmed"; // Confirmed, Cancelled

    // Navigation properties
    public Event Event { get; set; } = null!;
    public User User { get; set; } = null!;
}


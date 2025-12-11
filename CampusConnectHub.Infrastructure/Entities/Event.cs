namespace CampusConnectHub.Infrastructure.Entities;

public class Event
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime EventDate { get; set; }
    public string Location { get; set; } = string.Empty;
    public int CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public int? MaxAttendees { get; set; }

    // Navigation properties
    public User Creator { get; set; } = null!;
    public ICollection<EventRSVP> RSVPs { get; set; } = new List<EventRSVP>();
}


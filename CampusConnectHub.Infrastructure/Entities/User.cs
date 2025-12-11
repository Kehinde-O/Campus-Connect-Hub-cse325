namespace CampusConnectHub.Infrastructure.Entities;

public class User
{
    public int Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string Role { get; set; } = "Student"; // Student or Administrator
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public ICollection<NewsPost> NewsPosts { get; set; } = new List<NewsPost>();
    public ICollection<Event> CreatedEvents { get; set; } = new List<Event>();
    public ICollection<EventRSVP> EventRSVPs { get; set; } = new List<EventRSVP>();
}


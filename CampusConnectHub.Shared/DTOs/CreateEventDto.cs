using System.ComponentModel.DataAnnotations;

namespace CampusConnectHub.Shared.DTOs;

public class CreateEventDto
{
    [Required(ErrorMessage = "Title is required")]
    [MaxLength(200, ErrorMessage = "Title must be less than 200 characters")]
    public string Title { get; set; } = string.Empty;

    [Required(ErrorMessage = "Description is required")]
    public string Description { get; set; } = string.Empty;

    [Required(ErrorMessage = "Event date is required")]
    public DateTime EventDate { get; set; }

    [Required(ErrorMessage = "Location is required")]
    [MaxLength(200, ErrorMessage = "Location must be less than 200 characters")]
    public string Location { get; set; } = string.Empty;

    public int? MaxAttendees { get; set; }
}


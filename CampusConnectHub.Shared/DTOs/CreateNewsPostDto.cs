using System.ComponentModel.DataAnnotations;

namespace CampusConnectHub.Shared.DTOs;

public class CreateNewsPostDto
{
    [Required(ErrorMessage = "Title is required")]
    [MaxLength(200, ErrorMessage = "Title must be less than 200 characters")]
    public string Title { get; set; } = string.Empty;

    [Required(ErrorMessage = "Content is required")]
    public string Content { get; set; } = string.Empty;

    public bool IsPublished { get; set; } = true;
}


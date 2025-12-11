using System.ComponentModel.DataAnnotations;

namespace CampusConnectHub.Shared.DTOs;

public class CreateResourceDto
{
    [Required(ErrorMessage = "Title is required")]
    [MaxLength(200, ErrorMessage = "Title must be less than 200 characters")]
    public string Title { get; set; } = string.Empty;

    [MaxLength(500, ErrorMessage = "Description must be less than 500 characters")]
    public string Description { get; set; } = string.Empty;

    [Required(ErrorMessage = "URL is required")]
    [Url(ErrorMessage = "Invalid URL format")]
    [MaxLength(500, ErrorMessage = "URL must be less than 500 characters")]
    public string Url { get; set; } = string.Empty;

    [Required(ErrorMessage = "Category is required")]
    [MaxLength(100, ErrorMessage = "Category must be less than 100 characters")]
    public string Category { get; set; } = string.Empty;

    public int DisplayOrder { get; set; }
    public bool IsActive { get; set; } = true;
}


using System.ComponentModel.DataAnnotations;

namespace CampusConnectHub.Shared.DTOs;

public class UpdateUserRoleDto
{
    [Required(ErrorMessage = "Role is required")]
    [RegularExpression("(Student|Administrator)", ErrorMessage = "Role must be either 'Student' or 'Administrator'")]
    public string Role { get; set; } = string.Empty;
}


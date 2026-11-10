using System.ComponentModel.DataAnnotations;

namespace QuizMaster.Domain.DTOs.Admin;

public class ChangeUserRoleDTO
{
    [Required]
    public string Role { get; set; } = string.Empty;
}
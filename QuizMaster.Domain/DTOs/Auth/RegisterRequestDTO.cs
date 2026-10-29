
using System.ComponentModel.DataAnnotations;

namespace QuizMaster.Domain.DTOs.Auth;

public class RegisterRequestDTO
{
    [Required]
    [StringLength(100, MinimumLength = 3)]
    public string UserName { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    [MinLength(8)]
    public string Password { get; set; } = string.Empty;
}

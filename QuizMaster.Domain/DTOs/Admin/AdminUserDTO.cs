
namespace QuizMaster.Domain.DTOs.Admin;

public class AdminUserDTO
{
    public int Id { get; set; }

    public string UserName { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string Role { get; set; } = string.Empty;

    public string? RequestedRole { get; set; }

    public string ApprovalStatus { get; set; } = string.Empty;
}


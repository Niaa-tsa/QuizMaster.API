
namespace QuizMaster.Domain.Entities;

public class User
{
    public int Id { get; set; }

    public string UserName { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string PasswordHash { get; set; } = string.Empty;

    public int RoleId { get; set; }

    public Role Role { get; set; } = null!;

    public string? RequestedRole { get; set; }

    public string ApprovalStatus { get; set; } = "Approved";

    public ICollection<QuizAttempt> QuizAttempts { get; set; }
        = new List<QuizAttempt>();
}

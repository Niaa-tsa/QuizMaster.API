namespace QuizMaster.Domain.Entities;

public class QuizAttempt
{
    public int Id { get; set; }
    public int QuizId { get; set; }
    public int UserId { get; set; }
    public DateTime StartedAt { get; set; } = DateTime.UtcNow;
    public DateTime? FinishedAt { get; set; }
    public string Status { get; set; } = "InProgress";

    public Quiz Quiz { get; set; } = null!;
    public User User { get; set; } = null!;

    public ICollection<StudentAnswer> StudentAnswers { get; set; }
        = new List<StudentAnswer>();

    public QuizResult? QuizResult { get; set; }
}
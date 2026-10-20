namespace QuizMaster.Domain.Entities;

public class QuizResult
{
    public int Id { get; set; }
    public int AttemptId { get; set; }
    public decimal Score { get; set; }
    public decimal MaxScore { get; set; }

    public QuizAttempt QuizAttempt { get; set; } = null!;
}
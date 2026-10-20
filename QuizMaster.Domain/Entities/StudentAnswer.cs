namespace QuizMaster.Domain.Entities;

public class StudentAnswer
{
    public int Id { get; set; }
    public int AttemptId { get; set; }
    public int QuestionId { get; set; }
    public int AnswerOptionId { get; set; }

    public QuizAttempt QuizAttempt { get; set; } = null!;
    public Question Question { get; set; } = null!;
    public AnswerOption AnswerOption { get; set; } = null!;
}
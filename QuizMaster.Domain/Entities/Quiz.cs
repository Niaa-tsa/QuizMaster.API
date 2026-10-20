namespace QuizMaster.Domain.Entities;

public class Quiz
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public int TimeLimitMinutes { get; set; }
    public bool IsPublished { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<Question> Questions { get; set; }
        = new List<Question>();

    public ICollection<QuizAttempt> QuizAttempts { get; set; }
        = new List<QuizAttempt>();
}
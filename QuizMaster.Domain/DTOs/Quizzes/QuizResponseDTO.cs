namespace QuizMaster.Domain.DTOs.Quizzes;

public class QuizResponseDTO
{
    public int Id { get; set; }

    public string Title { get; set; } = string.Empty;

    public int TimeLimitMinutes { get; set; }

    public bool IsPublished { get; set; }

    public DateTime CreatedAt { get; set; }
}
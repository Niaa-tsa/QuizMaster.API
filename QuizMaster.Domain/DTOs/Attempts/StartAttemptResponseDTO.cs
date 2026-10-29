namespace QuizMaster.Domain.DTOs.Attempts;

public class StartAttemptResponseDTO
{
    public int AttemptId { get; set; }

    public int QuizId { get; set; }

    public string QuizTitle { get; set; } = string.Empty;

    public DateTime StartedAt { get; set; }

    public DateTime ExpiresAt { get; set; }
}
namespace QuizMaster.Domain.DTOs.Attempts;

public class AttemptHistoryResponseDTO
{
    public int AttemptId { get; set; }

    public int QuizId { get; set; }

    public string QuizTitle { get; set; } = string.Empty;

    public DateTime StartedAt { get; set; }

    public DateTime? FinishedAt { get; set; }

    public string Status { get; set; } = string.Empty;

    public decimal? Score { get; set; }

    public decimal? MaxScore { get; set; }
}
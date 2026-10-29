namespace QuizMaster.Domain.DTOs.Attempts;

public class FinishAttemptResponseDTO
{
    public int AttemptId { get; set; }

    public decimal Score { get; set; }

    public decimal MaxScore { get; set; }

    public DateTime FinishedAt { get; set; }
}
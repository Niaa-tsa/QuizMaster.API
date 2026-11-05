namespace QuizMaster.Domain.DTOs.Quizzes;

public class QuizStatisticsDTO
{
    public int QuizId { get; set; }
    public int AttemptsCount { get; set; }
    public decimal MaxScore { get; set; }
    public decimal AverageScore { get; set; }
    public decimal HighestScore { get; set; }
    public decimal LowestScore { get; set; }
    public decimal TotalScore { get; set; }
}
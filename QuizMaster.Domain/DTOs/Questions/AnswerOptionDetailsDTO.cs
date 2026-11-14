namespace QuizMaster.Domain.DTOs.Questions;

public class AnswerOptionDetailsDTO
{
    public int Id { get; set; }

    public int QuestionId { get; set; }

    public string Text { get; set; } = string.Empty;

    public bool IsCorrect { get; set; }
}
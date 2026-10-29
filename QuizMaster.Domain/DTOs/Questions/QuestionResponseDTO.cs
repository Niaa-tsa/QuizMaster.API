using QuizMaster.Domain.Enums;

namespace QuizMaster.Domain.DTOs.Questions;

public class QuestionResponseDTO
{
    public int Id { get; set; }

    public string Text { get; set; } = string.Empty;

    public QuestionType Type { get; set; }

    public decimal Points { get; set; }

    public List<AnswerOptionResponseDTO> AnswerOptions { get; set; }
        = new();
}

public class AnswerOptionResponseDTO
{
    public int Id { get; set; }

    public string Text { get; set; } = string.Empty;
}
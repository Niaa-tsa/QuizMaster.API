
using QuizMaster.Domain.Enums;

namespace QuizMaster.Domain.DTOs.Questions;

public class StudentQuestionDTO
{
    public int Id { get; set; }
    public string Text { get; set; } = string.Empty;
    public QuestionType Type { get; set; }
    public decimal Points { get; set; }

    public List<StudentAnswerOptionDTO> AnswerOptions { get; set; } = new();
}

public class StudentAnswerOptionDTO
{
    public int Id { get; set; }
    public string Text { get; set; } = string.Empty;
}

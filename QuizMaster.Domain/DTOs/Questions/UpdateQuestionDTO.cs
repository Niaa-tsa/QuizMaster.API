using System.ComponentModel.DataAnnotations;
using QuizMaster.Domain.Enums;

namespace QuizMaster.Domain.DTOs.Questions;

public class UpdateQuestionDTO
{
    [Required]
    [StringLength(1000, MinimumLength = 1)]
    public string Text { get; set; } = string.Empty;

    [EnumDataType(typeof(QuestionType))]
    public QuestionType Type { get; set; }

    [Range(typeof(decimal), "0.01", "999999.99")]
    public decimal Points { get; set; }

    [Required]
    [MinLength(2)]
    public List<AnswerOptionDTO> AnswerOptions { get; set; }
        = new();
}
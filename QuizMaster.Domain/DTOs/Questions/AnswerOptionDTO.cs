using System.ComponentModel.DataAnnotations;

namespace QuizMaster.Domain.DTOs.Questions;

public class AnswerOptionDTO
{
    public int? Id { get; set; }

    [Required]
    [StringLength(500, MinimumLength = 1)]
    public string Text { get; set; } = string.Empty;

    public bool IsCorrect { get; set; }
}
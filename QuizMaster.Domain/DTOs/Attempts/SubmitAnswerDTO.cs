using System.ComponentModel.DataAnnotations;

namespace QuizMaster.Domain.DTOs.Attempts;

public class SubmitAnswerDTO
{
    [Required]
    [Range(1, int.MaxValue)]
    public int QuestionId { get; set; }

    [Required]
    [MinLength(1)]
    public List<int> AnswerOptionIds { get; set; } = new();
}
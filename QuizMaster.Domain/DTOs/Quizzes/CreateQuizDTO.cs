using System.ComponentModel.DataAnnotations;

namespace QuizMaster.Domain.DTOs.Quizzes;

public class CreateQuizDTO
{
    [Required]
    [StringLength(200, MinimumLength = 3)]
    public string Title { get; set; } = string.Empty;

    [Range(1, 300)]
    public int TimeLimitMinutes { get; set; }
}
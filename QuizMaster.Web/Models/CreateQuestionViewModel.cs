using System.ComponentModel.DataAnnotations;
using QuizMaster.Domain.Enums;

namespace QuizMaster.Web.Models
{
    public class CreateQuestionViewModel
    {
        public int QuizId { get; set; }

        [Required(ErrorMessage = "Question text is required.")]
        public string Text { get; set; } = string.Empty;

        [Required]
        public QuestionType Type { get; set; }

        [Range(0.1, 100, ErrorMessage = "Points must be between 0.1 and 100.")]
        public decimal Points { get; set; } = 1;

        public List<CreateAnswerOptionViewModel> AnswerOptions { get; set; }
            = new List<CreateAnswerOptionViewModel>();
    }

    public class CreateAnswerOptionViewModel
    {
        [Required(ErrorMessage = "Answer text is required.")]
        public string Text { get; set; } = string.Empty;

        public bool IsCorrect { get; set; }
    }
}
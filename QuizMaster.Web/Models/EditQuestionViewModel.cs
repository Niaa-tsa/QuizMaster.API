using System.ComponentModel.DataAnnotations;
using QuizMaster.Domain.Enums;

namespace QuizMaster.Web.Models
{
    public class EditQuestionViewModel
    {
        public int Id { get; set; }

        public int QuizId { get; set; }

        [Required(ErrorMessage = "Question text is required.")]
        public string Text { get; set; } = string.Empty;

        [Required]
        public QuestionType Type { get; set; }

        [Range(0.1, 100, ErrorMessage = "Points must be between 0.1 and 100.")]
        public decimal Points { get; set; } = 1;

        public List<EditAnswerOptionViewModel> AnswerOptions { get; set; }
            = new List<EditAnswerOptionViewModel>();
    }

    public class EditAnswerOptionViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Answer text is required.")]
        public string Text { get; set; } = string.Empty;

        public bool IsCorrect { get; set; }
    }
}
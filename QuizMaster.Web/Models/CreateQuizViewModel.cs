using System.ComponentModel.DataAnnotations;

namespace QuizMaster.Web.Models
{
    public class CreateQuizViewModel
    {
        [Required(ErrorMessage = "Quiz title is required.")]
        public string Title { get; set; } = string.Empty;

        [Required]
        [Range(1, 300, ErrorMessage = "Time limit must be between 1 and 300 minutes.")]
        public int TimeLimitMinutes { get; set; }
    }
}
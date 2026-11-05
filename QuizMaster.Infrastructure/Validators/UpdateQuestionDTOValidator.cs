using FluentValidation;
using QuizMaster.Domain.DTOs.Questions;
using QuizMaster.Domain.Enums;

namespace QuizMaster.Infrastructure.Validators;

public class UpdateQuestionDTOValidator : AbstractValidator<UpdateQuestionDTO>
{
    public UpdateQuestionDTOValidator()
    {
        RuleFor(x => x)
            .Must(HaveValidCorrectAnswers)
            .WithMessage("Invalid answer options for this question type.");
    }

    private static bool HaveValidCorrectAnswers(UpdateQuestionDTO dto)
    {
        var correctCount = dto.AnswerOptions.Count(o => o.IsCorrect);

        return dto.Type switch
        {
            QuestionType.SingleChoice => correctCount == 1,
            QuestionType.MultipleChoice => correctCount >= 1,
            QuestionType.TrueFalse =>
                dto.AnswerOptions.Count == 2 && correctCount == 1,
            _ => false
        };
    }
}
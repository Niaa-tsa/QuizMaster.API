using QuizMaster.Domain.Entities;
using QuizMaster.Domain.Enums;

namespace QuizMaster.Service.Rules;

/// <summary>
/// კითხვის ტიპის წესები ერთ ადგილას.
/// იყენებს QuestionService (შექმნა/რედაქტირება) და QuizService (გამოქვეყნება).
/// </summary>
public static class QuestionRules
{
    public static bool IsValid(Question question)
    {
        if (question.AnswerOptions == null ||
            question.AnswerOptions.Count == 0)
            return false;

        var correctAnswers = question.AnswerOptions
            .Count(a => a.IsCorrect);

        return question.Type switch
        {
            QuestionType.SingleChoice => correctAnswers == 1,
            QuestionType.MultipleChoice => correctAnswers >= 2,
            QuestionType.TrueFalse =>
                question.AnswerOptions.Count == 2 && correctAnswers == 1,
            _ => false
        };
    }
}
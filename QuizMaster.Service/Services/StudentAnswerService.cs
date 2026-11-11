using QuizMaster.Domain.Entities;
using QuizMaster.Domain.DTOs.Attempts;
using QuizMaster.Domain.Enums;
using QuizMaster.Service.Interfaces;
using QuizMaster.Service.Interfaces.Repositories;

namespace QuizMaster.Service.Services;

public class StudentAnswerService : IStudentAnswerService
{
    private readonly IStudentAnswerRepository _repository;
    private readonly IQuizAttemptRepository _attemptRepository;
    private readonly IQuestionService _questionService;
    private readonly IQuizAttemptService _quizAttemptService;

    public StudentAnswerService(
        IStudentAnswerRepository repository,
        IQuizAttemptRepository attemptRepository,
        IQuestionService questionService,
        IQuizAttemptService quizAttemptService)
    {
        _repository = repository;
        _attemptRepository = attemptRepository;
        _questionService = questionService;
        _quizAttemptService = quizAttemptService;
    }

    public async Task<bool> SubmitAnswerAsync(
        int attemptId,
        int userId,
        SubmitAnswerDTO dto)
    {
        if (dto == null)
            return false;
        var attempt = await _attemptRepository
            .GetStudentAttemptAsync(attemptId, userId);

        if (attempt == null || attempt.Status != "InProgress")
            return false;

        if (attempt.Quiz == null)
            return false;

        var timeLimit = TimeSpan.FromMinutes(
            attempt.Quiz.TimeLimitMinutes);

        var elapsedTime = DateTime.UtcNow - attempt.StartedAt;

        if (elapsedTime >= timeLimit)
        {
            await _quizAttemptService.FinishAttemptAsync(
                attemptId, userId);

            return false;
        }

        var question = await _questionService.GetByIdAsync(dto.QuestionId);

        if (question == null || question.QuizId != attempt.QuizId)
            return false;

        if (dto.AnswerOptionIds == null ||
            dto.AnswerOptionIds.Count == 0)
            return false;

        var selectedIds = dto.AnswerOptionIds.Distinct().ToList();

        if (selectedIds.Count != dto.AnswerOptionIds.Count)
            return false;

        if (!question.AnswerOptions.Any())
            return false;

        if (!question.AnswerOptions.Any(a => a.IsCorrect))
            return false;
        if (question.Type == QuestionType.MultipleChoice &&
    question.AnswerOptions.Count(a => a.IsCorrect) < 2)
            return false;
        if (question.Type == QuestionType.TrueFalse &&
    question.AnswerOptions.Count != 2)
            return false;
        var validOptionIds = question.AnswerOptions
            .Select(a => a.Id)
            .ToHashSet();

        if (selectedIds.Any(id => !validOptionIds.Contains(id)))
            return false;

        if ((question.Type == QuestionType.SingleChoice ||
             question.Type == QuestionType.TrueFalse) &&
            selectedIds.Count != 1)
            return false;
        if (question.Type != QuestionType.SingleChoice &&
    question.Type != QuestionType.MultipleChoice &&
    question.Type != QuestionType.TrueFalse)
            return false;
        var existingAnswers = await _repository
            .GetAnswersByAttemptAndQuestionAsync(
                attemptId, dto.QuestionId);

        if (existingAnswers.Any())
            return false;

        foreach (var optionId in selectedIds)
        {
            var studentAnswer = new StudentAnswer
            {
                AttemptId = attemptId,
                QuestionId = dto.QuestionId,
                AnswerOptionId = optionId
            };

            await _repository.AddStudentAnswerAsync(studentAnswer);
        }

        await _repository.SaveChangesAsync();

        return true;
    }
}
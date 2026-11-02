using QuizMaster.Domain.Entities;
using QuizMaster.Service.Interfaces;
using QuizMaster.Service.Interfaces.Repositories;

namespace QuizMaster.Service.Services;

public class QuizAttemptService : IQuizAttemptService
{
    private readonly IQuizAttemptRepository _repository;
    private readonly IQuizRepository _quizRepository;
    private readonly IStudentAnswerRepository _studentAnswerRepository;
    private readonly IQuizResultRepository _quizResultRepository;

    public QuizAttemptService(
        IQuizAttemptRepository repository,
        IQuizRepository quizRepository,
        IStudentAnswerRepository studentAnswerRepository,
        IQuizResultRepository quizResultRepository)
    {
        _repository = repository;
        _quizRepository = quizRepository;
        _studentAnswerRepository = studentAnswerRepository;
        _quizResultRepository = quizResultRepository;
    }

    public async Task<QuizAttempt?> GetStudentAttemptAsync(
        int attemptId,
        int userId)
    {
        return await _repository.GetStudentAttemptAsync(
            attemptId, userId);
    }

    public async Task<QuizAttempt?> StartAsync(
        int quizId,
        int userId)
    {
        var quiz = await _quizRepository.GetQuizByIdAsync(quizId);

        if (quiz == null || !quiz.IsPublished)
            return null;

        var existingAttempts =
            await _repository.GetStudentAttemptsAsync(userId);

        if (existingAttempts.Any(a => a.QuizId == quizId))
            return null;

        var attempt = new QuizAttempt
        {
            QuizId = quizId,
            UserId = userId,
            StartedAt = DateTime.UtcNow,
            Status = "InProgress"
        };

        await _repository.AddQuizAttemptAsync(attempt);
        await _repository.SaveChangesAsync();

        return attempt;
    }

    public async Task<QuizResult?> FinishAttemptAsync(
        int attemptId,
        int userId)
    {
        var attempt = await _repository.GetStudentAttemptAsync(
            attemptId, userId);

        if (attempt == null || attempt.Status != "InProgress")
            return null;

        if (attempt.Quiz == null)
            return null;

        var questions = attempt.Quiz.Questions;

        decimal score = 0;
        decimal maxScore = 0;

        foreach (var question in questions)
        {
            maxScore += question.Points;

            var studentAnswers =
                await _studentAnswerRepository
                    .GetAnswersByAttemptAndQuestionAsync(
                        attemptId, question.Id);

            var selectedIds = studentAnswers
                .Select(a => a.AnswerOptionId)
                .ToHashSet();

            var correctIds = question.AnswerOptions
                .Where(a => a.IsCorrect)
                .Select(a => a.Id)
                .ToHashSet();

            if (correctIds.Count > 0 &&
                selectedIds.Count > 0 &&
                selectedIds.SetEquals(correctIds))
            {
                score += question.Points;
            }
        }

        var result = new QuizResult
        {
            AttemptId = attemptId,
            Score = score,
            MaxScore = maxScore
        };

        await _quizResultRepository.AddQuizResultAsync(result);

        attempt.Status = "Finished";
        attempt.FinishedAt = DateTime.UtcNow;

        await _repository.UpdateQuizAttemptAsync(
            attempt.Id, attempt);

        await _repository.SaveChangesAsync();

        return result;
    }

    public async Task<List<QuizAttempt>> GetStudentAttemptsAsync(
        int userId)
    {
        var attempts =
            await _repository.GetStudentAttemptsAsync(userId);

        return attempts.ToList();
    }
}
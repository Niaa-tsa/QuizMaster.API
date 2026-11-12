using QuizMaster.Domain.DTOs.Quizzes;
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
    private readonly IUnitOfWork _unitOfWork;

    public QuizAttemptService(
    IQuizAttemptRepository repository,
    IQuizRepository quizRepository,
    IStudentAnswerRepository studentAnswerRepository,
    IQuizResultRepository quizResultRepository,
    IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _quizRepository = quizRepository;
        _studentAnswerRepository = studentAnswerRepository;
        _quizResultRepository = quizResultRepository;
        _unitOfWork = unitOfWork;
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
        await _unitOfWork.SaveChangesAsync();

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

        var now = DateTime.UtcNow;

        var deadline = attempt.StartedAt.AddMinutes(
            attempt.Quiz.TimeLimitMinutes);

        var finishedAt = now > deadline ? deadline : now;

        var result = new QuizResult
        {
            AttemptId = attemptId,
            Score = score,
            MaxScore = maxScore
        };

        await _quizResultRepository.AddQuizResultAsync(result);

        attempt.Status = "Finished";
        attempt.FinishedAt = finishedAt;

        await _repository.UpdateQuizAttemptAsync(
            attempt.Id, attempt);

        await _unitOfWork.SaveChangesAsync();

        return result;
    }

    public async Task<List<QuizAttempt>> GetStudentAttemptsAsync(
        int userId)
    {
        var attempts =
            await _repository.GetStudentAttemptsAsync(userId);

        return attempts.ToList();
    }
    public async Task<List<LeaderboardEntryDTO>?> GetLeaderboardAsync(
    int quizId,
    int page,
    int pageSize)
    {
        var quiz = await _quizRepository.GetQuizByIdAsync(quizId);

        if (quiz == null)
            return null;

        var results = await _quizResultRepository
            .GetLeaderboardAsync(quizId, page, pageSize);

        var startRank = (page - 1) * pageSize;

        return results
            .Select((r, index) => new LeaderboardEntryDTO
            {
                Rank = startRank + index + 1,
                UserName = r.QuizAttempt.User.UserName,
                Score = r.Score,
                MaxScore = r.MaxScore,
                FinishedAt = r.QuizAttempt.FinishedAt.GetValueOrDefault()
            })
            .ToList();
    }
    public async Task<QuizStatisticsDTO?> GetQuizStatisticsAsync(
     int quizId,
     int lecturerId)
    {
        var quiz = await _quizRepository.GetQuizByIdAsync(quizId);

        if (quiz == null || quiz.LecturerId != lecturerId)
            return null;

        var statistics = await _quizResultRepository
            .GetQuizStatisticsAsync(quizId);

        if (statistics == null)
            return new QuizStatisticsDTO { QuizId = quizId };

        statistics.AverageScore = Math.Round(statistics.AverageScore, 2);

        return statistics;
    }
}
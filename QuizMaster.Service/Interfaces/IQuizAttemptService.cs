using QuizMaster.Domain.DTOs.Quizzes;
using QuizMaster.Domain.Entities;

namespace QuizMaster.Service.Interfaces;

public interface IQuizAttemptService
{
    Task<QuizAttempt?> GetStudentAttemptAsync(
        int attemptId,
        int userId);

    Task<QuizAttempt?> StartAsync(
        int quizId,
        int userId);

    Task<List<QuizAttempt>> GetStudentAttemptsAsync(
        int userId);

    Task<QuizResult?> FinishAttemptAsync(
        int attemptId,
        int userId);
    Task<List<LeaderboardEntryDTO>?> GetLeaderboardAsync(
    int quizId,
    int page,
    int pageSize);
    Task<QuizStatisticsDTO?> GetQuizStatisticsAsync(int quizId);
}
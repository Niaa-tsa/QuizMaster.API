using QuizMaster.Domain.DTOs.Quizzes;
using QuizMaster.Domain.Entities;

namespace QuizMaster.Service.Interfaces.Repositories;

public interface IQuizResultRepository
{
    Task AddQuizResultAsync(QuizResult quizResult);
    Task<ICollection<QuizResult>> GetLeaderboardAsync(
    int quizId,
    int page,
    int pageSize);
    Task<QuizStatisticsDTO?> GetQuizStatisticsAsync(int quizId);
    Task SaveChangesAsync();
}

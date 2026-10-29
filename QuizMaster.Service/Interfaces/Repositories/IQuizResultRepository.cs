using QuizMaster.Domain.Entities;

namespace QuizMaster.Service.Interfaces.Repositories
{
    public interface IQuizResultRepository
    {
        Task AddQuizResultAsync(QuizResult quizResult);
        Task<ICollection<QuizResult>> GetAllQuizResultsAsync();
        Task<QuizResult?> GetQuizResultByIdAsync(int id);
        Task UpdateQuizResultAsync(int id, QuizResult quizResult);
        Task DeleteQuizResultAsync(int id);
        Task SaveChangesAsync();
    }
}
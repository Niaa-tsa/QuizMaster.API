using QuizMaster.Domain.Entities;

namespace QuizMaster.Service.Interfaces.Repositories
{
    public interface IQuizAttemptRepository
    {
        Task AddQuizAttemptAsync(QuizAttempt quizAttempt);
        Task<ICollection<QuizAttempt>> GetAllQuizAttemptsAsync();
        Task<QuizAttempt?> GetQuizAttemptByIdAsync(int id);
        Task UpdateQuizAttemptAsync(int id, QuizAttempt quizAttempt);
        Task DeleteQuizAttemptAsync(int id);
        Task SaveChangesAsync();
    }
}
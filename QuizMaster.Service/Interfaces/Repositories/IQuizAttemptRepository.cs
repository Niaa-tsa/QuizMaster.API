using QuizMaster.Domain.Entities;

namespace QuizMaster.Service.Interfaces.Repositories
{
    public interface IQuizAttemptRepository
    {
        Task AddQuizAttemptAsync(QuizAttempt quizAttempt);

        Task UpdateQuizAttemptAsync(
            int id,
            QuizAttempt quizAttempt);

        Task<QuizAttempt?> GetStudentAttemptAsync(
            int attemptId,
            int userId);

        Task<bool> HasAttemptsForQuizAsync(int quizId);

        Task<ICollection<QuizAttempt>> GetStudentAttemptsAsync(
            int userId);

        Task SaveChangesAsync();
    }
}
using QuizMaster.Domain.Entities;

namespace QuizMaster.Service.Interfaces;

public interface IQuizAttemptService
{
    Task<List<QuizAttempt>> GetAllAsync();
    Task<QuizAttempt?> GetByIdAsync(int id);
    Task AddAsync(QuizAttempt quizAttempt);
    Task<bool> UpdateAsync(QuizAttempt quizAttempt);
    Task<bool> DeleteAsync(int id);
}
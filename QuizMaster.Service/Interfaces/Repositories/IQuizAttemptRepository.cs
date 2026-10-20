using QuizMaster.Domain.Entities;

namespace QuizMaster.Service.Interfaces.Repositories;

public interface IQuizAttemptRepository
{
    Task<List<QuizAttempt>> GetAllAsync();
    Task<QuizAttempt?> GetByIdAsync(int id);
    Task AddAsync(QuizAttempt quizAttempt);
    void Update(QuizAttempt quizAttempt);
    void Delete(QuizAttempt quizAttempt);
    Task SaveChangesAsync();
}
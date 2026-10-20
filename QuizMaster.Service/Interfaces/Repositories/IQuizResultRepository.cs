using QuizMaster.Domain.Entities;

namespace QuizMaster.Service.Interfaces.Repositories;

public interface IQuizResultRepository
{
    Task<List<QuizResult>> GetAllAsync();
    Task<QuizResult?> GetByIdAsync(int id);
    Task AddAsync(QuizResult quizResult);
    void Update(QuizResult quizResult);
    void Delete(QuizResult quizResult);
    Task SaveChangesAsync();
}
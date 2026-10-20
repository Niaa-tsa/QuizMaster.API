using QuizMaster.Domain.Entities;

namespace QuizMaster.Service.Interfaces;

public interface IQuizResultService
{
    Task<List<QuizResult>> GetAllAsync();
    Task<QuizResult?> GetByIdAsync(int id);
    Task AddAsync(QuizResult quizResult);
    Task<bool> UpdateAsync(QuizResult quizResult);
    Task<bool> DeleteAsync(int id);
}
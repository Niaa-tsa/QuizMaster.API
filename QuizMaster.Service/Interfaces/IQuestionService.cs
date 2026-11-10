using QuizMaster.Domain.Entities;

namespace QuizMaster.Service.Interfaces;

public interface IQuestionService
{
    Task<List<Question>> GetAllAsync();
    Task<Question?> GetByIdAsync(int id);
    Task<List<Question>> GetByQuizIdAsync(int quizId);
    Task AddAsync(Question question);
    Task<bool> UpdateAsync(Question question);
    Task<bool> DeleteAsync(int id);
    Task<List<Question>> GetQuestionsByLecturerIdAsync(int lecturerId);
}
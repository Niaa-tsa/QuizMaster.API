using QuizMaster.Domain.Entities;

namespace QuizMaster.Service.Interfaces;

public interface IStudentAnswerService
{
    Task<List<StudentAnswer>> GetAllAsync();
    Task<StudentAnswer?> GetByIdAsync(int id);
    Task AddAsync(StudentAnswer studentAnswer);
    Task<bool> UpdateAsync(StudentAnswer studentAnswer);
    Task<bool> DeleteAsync(int id);
}
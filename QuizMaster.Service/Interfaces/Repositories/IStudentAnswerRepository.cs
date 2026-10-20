using QuizMaster.Domain.Entities;

namespace QuizMaster.Service.Interfaces.Repositories;

public interface IStudentAnswerRepository
{
    Task<List<StudentAnswer>> GetAllAsync();
    Task<StudentAnswer?> GetByIdAsync(int id);
    Task AddAsync(StudentAnswer studentAnswer);
    void Update(StudentAnswer studentAnswer);
    void Delete(StudentAnswer studentAnswer);
    Task SaveChangesAsync();
}
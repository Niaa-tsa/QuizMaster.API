using QuizMaster.Domain.Entities;

namespace QuizMaster.Service.Interfaces.Repositories
{
    public interface IStudentAnswerRepository
    {
        Task AddStudentAnswerAsync(StudentAnswer studentAnswer);
        Task<ICollection<StudentAnswer>> GetAllStudentAnswersAsync();
        Task<StudentAnswer?> GetStudentAnswerByIdAsync(int id);
        Task UpdateStudentAnswerAsync(int id, StudentAnswer studentAnswer);
        Task DeleteStudentAnswerAsync(int id);
        Task SaveChangesAsync();
    }
}
using QuizMaster.Domain.Entities;

namespace QuizMaster.Service.Interfaces.Repositories
{
    public interface IQuestionRepository
    {
        Task AddQuestionAsync(Question question);
        Task<ICollection<Question>> GetAllQuestionsAsync();
        Task<Question?> GetQuestionByIdAsync(int id);
        Task UpdateQuestionAsync(int id, Question question);
        Task DeleteQuestionAsync(int id);
        Task SaveChangesAsync();
    }
}
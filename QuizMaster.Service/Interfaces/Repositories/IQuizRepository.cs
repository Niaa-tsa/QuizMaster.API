using QuizMaster.Domain.Entities;

namespace QuizMaster.Service.Interfaces.Repositories
{
    public interface IQuizRepository
    {
        Task AddQuizAsync(Quiz quiz);
        Task<ICollection<Quiz>> GetAllQuizzesAsync();
        Task<Quiz?> GetQuizByIdAsync(int id);
        Task UpdateQuizAsync(int id, Quiz quiz);
        Task DeleteQuizAsync(int id);
        Task SaveChangesAsync();
        Task<ICollection<Quiz>> GetPublishedQuizzesAsync();
    }
}
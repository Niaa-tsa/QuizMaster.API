using QuizMaster.Domain.Entities;

namespace QuizMaster.Service.Interfaces.Repositories
{
    public interface IAnswerOptionRepository
    {
        Task AddAnswerOptionAsync(AnswerOption answerOption);
        Task<ICollection<AnswerOption>> GetAllAnswerOptionsAsync();
        Task<AnswerOption?> GetAnswerOptionByIdAsync(int id);
        Task UpdateAnswerOptionAsync(int id, AnswerOption answerOption);
        Task DeleteAnswerOptionAsync(int id);
        Task SaveChangesAsync();
    }
}
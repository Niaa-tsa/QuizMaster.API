using QuizMaster.Domain.Entities;

namespace QuizMaster.Service.Interfaces.Repositories;

public interface IAnswerOptionRepository
{
    Task<List<AnswerOption>> GetAllAsync();
    Task<AnswerOption?> GetByIdAsync(int id);
    Task AddAsync(AnswerOption answerOption);
    void Update(AnswerOption answerOption);
    void Delete(AnswerOption answerOption);
    Task SaveChangesAsync();
}
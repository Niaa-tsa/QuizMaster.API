using QuizMaster.Domain.Entities;

namespace QuizMaster.Service.Interfaces;

public interface IAnswerOptionService
{
    Task<List<AnswerOption>> GetAllAsync();
    Task<AnswerOption?> GetByIdAsync(int id);
    Task AddAsync(AnswerOption answerOption);
    Task<bool> UpdateAsync(AnswerOption answerOption);
    Task<bool> DeleteAsync(int id);
}
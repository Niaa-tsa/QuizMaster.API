using QuizMaster.Domain.Entities;
using QuizMaster.Service.Interfaces;
using QuizMaster.Service.Interfaces.Repositories;

namespace QuizMaster.Service.Services;

public class QuestionService : IQuestionService
{
    private readonly IQuestionRepository _repository;

    public QuestionService(IQuestionRepository repository)
    {
        _repository = repository;
    }

    public Task<List<Question>> GetAllAsync() =>
        _repository.GetAllAsync();

    public Task<Question?> GetByIdAsync(int id) =>
        _repository.GetByIdAsync(id);

    public async Task AddAsync(Question question)
    {
        await _repository.AddAsync(question);
        await _repository.SaveChangesAsync();
    }

    public async Task<bool> UpdateAsync(Question question)
    {
        if (await _repository.GetByIdAsync(question.Id) is null)
            return false;

        _repository.Update(question);
        await _repository.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var question = await _repository.GetByIdAsync(id);

        if (question is null)
            return false;

        _repository.Delete(question);
        await _repository.SaveChangesAsync();
        return true;
    }
}
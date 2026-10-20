using QuizMaster.Domain.Entities;
using QuizMaster.Service.Interfaces;
using QuizMaster.Service.Interfaces.Repositories;

namespace QuizMaster.Service.Services;

public class AnswerOptionService : IAnswerOptionService
{
    private readonly IAnswerOptionRepository _repository;

    public AnswerOptionService(IAnswerOptionRepository repository)
    {
        _repository = repository;
    }

    public Task<List<AnswerOption>> GetAllAsync() =>
        _repository.GetAllAsync();

    public Task<AnswerOption?> GetByIdAsync(int id) =>
        _repository.GetByIdAsync(id);

    public async Task AddAsync(AnswerOption answerOption)
    {
        await _repository.AddAsync(answerOption);
        await _repository.SaveChangesAsync();
    }

    public async Task<bool> UpdateAsync(AnswerOption answerOption)
    {
        if (await _repository.GetByIdAsync(answerOption.Id) is null)
            return false;

        _repository.Update(answerOption);
        await _repository.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var answerOption = await _repository.GetByIdAsync(id);

        if (answerOption is null)
            return false;

        _repository.Delete(answerOption);
        await _repository.SaveChangesAsync();
        return true;
    }
}
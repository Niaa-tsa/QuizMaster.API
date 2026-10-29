
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

    public async Task<List<AnswerOption>> GetAllAsync()
    {
        var answerOptions =
            await _repository.GetAllAnswerOptionsAsync();

        return answerOptions.ToList();
    }

    public Task<AnswerOption?> GetByIdAsync(int id)
    {
        return _repository.GetAnswerOptionByIdAsync(id);
    }

    public async Task AddAsync(AnswerOption answerOption)
    {
        await _repository.AddAnswerOptionAsync(answerOption);
        await _repository.SaveChangesAsync();
    }

    public async Task<bool> UpdateAsync(AnswerOption answerOption)
    {
        var existingOption =
            await _repository.GetAnswerOptionByIdAsync(answerOption.Id);

        if (existingOption is null)
            return false;

        await _repository.UpdateAnswerOptionAsync(
            answerOption.Id, answerOption);

        await _repository.SaveChangesAsync();

        return true;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var answerOption =
            await _repository.GetAnswerOptionByIdAsync(id);

        if (answerOption is null)
            return false;

        await _repository.DeleteAnswerOptionAsync(id);
        await _repository.SaveChangesAsync();

        return true;
    }
}

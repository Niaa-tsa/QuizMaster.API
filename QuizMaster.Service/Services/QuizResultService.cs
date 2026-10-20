using QuizMaster.Domain.Entities;
using QuizMaster.Service.Interfaces;
using QuizMaster.Service.Interfaces.Repositories;

namespace QuizMaster.Service.Services;

public class QuizResultService : IQuizResultService
{
    private readonly IQuizResultRepository _repository;

    public QuizResultService(IQuizResultRepository repository)
    {
        _repository = repository;
    }

    public Task<List<QuizResult>> GetAllAsync() =>
        _repository.GetAllAsync();

    public Task<QuizResult?> GetByIdAsync(int id) =>
        _repository.GetByIdAsync(id);

    public async Task AddAsync(QuizResult quizResult)
    {
        await _repository.AddAsync(quizResult);
        await _repository.SaveChangesAsync();
    }

    public async Task<bool> UpdateAsync(QuizResult quizResult)
    {
        if (await _repository.GetByIdAsync(quizResult.Id) is null)
            return false;

        _repository.Update(quizResult);
        await _repository.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var quizResult = await _repository.GetByIdAsync(id);

        if (quizResult is null)
            return false;

        _repository.Delete(quizResult);
        await _repository.SaveChangesAsync();
        return true;
    }
}

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

    public async Task<List<QuizResult>> GetAllAsync()
    {
        var results = await _repository.GetAllQuizResultsAsync();
        return results.ToList();
    }

    public Task<QuizResult?> GetByIdAsync(int id)
    {
        return _repository.GetQuizResultByIdAsync(id);
    }

    public async Task AddAsync(QuizResult quizResult)
    {
        await _repository.AddQuizResultAsync(quizResult);
        await _repository.SaveChangesAsync();
    }

    public async Task<bool> UpdateAsync(QuizResult quizResult)
    {
        var existingResult =
            await _repository.GetQuizResultByIdAsync(quizResult.Id);

        if (existingResult is null)
            return false;

        await _repository.UpdateQuizResultAsync(
            quizResult.Id, quizResult);

        await _repository.SaveChangesAsync();

        return true;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var quizResult = await _repository.GetQuizResultByIdAsync(id);

        if (quizResult is null)
            return false;

        await _repository.DeleteQuizResultAsync(id);
        await _repository.SaveChangesAsync();

        return true;
    }
}

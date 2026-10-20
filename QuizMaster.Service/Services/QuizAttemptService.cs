using QuizMaster.Domain.Entities;
using QuizMaster.Service.Interfaces;
using QuizMaster.Service.Interfaces.Repositories;

namespace QuizMaster.Service.Services;

public class QuizAttemptService : IQuizAttemptService
{
    private readonly IQuizAttemptRepository _repository;

    public QuizAttemptService(IQuizAttemptRepository repository)
    {
        _repository = repository;
    }

    public Task<List<QuizAttempt>> GetAllAsync() =>
        _repository.GetAllAsync();

    public Task<QuizAttempt?> GetByIdAsync(int id) =>
        _repository.GetByIdAsync(id);

    public async Task AddAsync(QuizAttempt quizAttempt)
    {
        await _repository.AddAsync(quizAttempt);
        await _repository.SaveChangesAsync();
    }

    public async Task<bool> UpdateAsync(QuizAttempt quizAttempt)
    {
        if (await _repository.GetByIdAsync(quizAttempt.Id) is null)
            return false;

        _repository.Update(quizAttempt);
        await _repository.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var quizAttempt = await _repository.GetByIdAsync(id);

        if (quizAttempt is null)
            return false;

        _repository.Delete(quizAttempt);
        await _repository.SaveChangesAsync();
        return true;
    }
}
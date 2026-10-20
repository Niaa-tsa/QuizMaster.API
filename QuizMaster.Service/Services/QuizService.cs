using QuizMaster.Domain.Entities;
using QuizMaster.Service.Interfaces;
using QuizMaster.Service.Interfaces.Repositories;

namespace QuizMaster.Service.Services;

public class QuizService : IQuizService
{
    private readonly IQuizRepository _repository;

    public QuizService(IQuizRepository repository)
    {
        _repository = repository;
    }

    public Task<List<Quiz>> GetAllAsync() =>
        _repository.GetAllAsync();

    public Task<Quiz?> GetByIdAsync(int id) =>
        _repository.GetByIdAsync(id);

    public async Task AddAsync(Quiz quiz)
    {
        await _repository.AddAsync(quiz);
        await _repository.SaveChangesAsync();
    }

    public async Task<bool> UpdateAsync(Quiz quiz)
    {
        if (await _repository.GetByIdAsync(quiz.Id) is null)
            return false;

        _repository.Update(quiz);
        await _repository.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var quiz = await _repository.GetByIdAsync(id);

        if (quiz is null)
            return false;

        _repository.Delete(quiz);
        await _repository.SaveChangesAsync();
        return true;
    }
}

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

    public async Task<List<Quiz>> GetAllAsync()
    {
        var quizzes = await _repository.GetAllQuizzesAsync();
        return quizzes.ToList();
    }

    public Task<Quiz?> GetByIdAsync(int id)
    {
        return _repository.GetQuizByIdAsync(id);
    }

    public async Task AddAsync(Quiz quiz)
    {
        await _repository.AddQuizAsync(quiz);
        await _repository.SaveChangesAsync();
    }

    public async Task<bool> UpdateAsync(Quiz quiz)
    {
        var existingQuiz =
            await _repository.GetQuizByIdAsync(quiz.Id);

        if (existingQuiz is null)
            return false;

        await _repository.UpdateQuizAsync(quiz.Id, quiz);
        await _repository.SaveChangesAsync();

        return true;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var quiz = await _repository.GetQuizByIdAsync(id);

        if (quiz is null)
            return false;

        await _repository.DeleteQuizAsync(id);
        await _repository.SaveChangesAsync();

        return true;
    }

    public async Task<bool> PublishAsync(int id)
    {
        var quiz = await _repository.GetQuizByIdAsync(id);

        if (quiz is null)
            return false;

        quiz.IsPublished = true;

        await _repository.UpdateQuizAsync(id, quiz);
        await _repository.SaveChangesAsync();

        return true;
    }

    public async Task<bool> CloseAsync(int id)
    {
        var quiz = await _repository.GetQuizByIdAsync(id);

        if (quiz is null)
            return false;

        quiz.IsPublished = false;

        await _repository.UpdateQuizAsync(id, quiz);
        await _repository.SaveChangesAsync();

        return true;
    }

    public async Task<List<Quiz>> GetPublishedAsync()
    {
        var quizzes = await _repository.GetPublishedQuizzesAsync();
        return quizzes.ToList();
    }


}

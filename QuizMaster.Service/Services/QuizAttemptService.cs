
using QuizMaster.Domain.Entities;
using QuizMaster.Service.Interfaces;
using QuizMaster.Service.Interfaces.Repositories;

namespace QuizMaster.Service.Services;

public class QuizAttemptService : IQuizAttemptService
{
    private readonly IQuizAttemptRepository _repository;
    private readonly IQuizRepository _quizRepository;
    public QuizAttemptService(
     IQuizAttemptRepository repository,
     IQuizRepository quizRepository)
    {
        _repository = repository;
        _quizRepository = quizRepository;
    }

    public async Task<List<QuizAttempt>> GetAllAsync()
    {
        var attempts = await _repository.GetAllQuizAttemptsAsync();
        return attempts.ToList();
    }

    public Task<QuizAttempt?> GetByIdAsync(int id)
    {
        return _repository.GetQuizAttemptByIdAsync(id);
    }

    public async Task AddAsync(QuizAttempt quizAttempt)
    {
        await _repository.AddQuizAttemptAsync(quizAttempt);
        await _repository.SaveChangesAsync();
    }

    public async Task<bool> UpdateAsync(QuizAttempt quizAttempt)
    {
        var existingAttempt =
            await _repository.GetQuizAttemptByIdAsync(quizAttempt.Id);

        if (existingAttempt is null)
            return false;

        await _repository.UpdateQuizAttemptAsync(
            quizAttempt.Id, quizAttempt);

        await _repository.SaveChangesAsync();

        return true;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var quizAttempt = await _repository.GetQuizAttemptByIdAsync(id);

        if (quizAttempt is null)
            return false;

        await _repository.DeleteQuizAttemptAsync(id);
        await _repository.SaveChangesAsync();

        return true;
    }

    public async Task<QuizAttempt?> StartAsync(int quizId, int userId)
    {
        var quiz = await _quizRepository.GetQuizByIdAsync(quizId);

        if (quiz is null || !quiz.IsPublished)
            return null;

        var attempt = new QuizAttempt
        {
            QuizId = quizId,
            UserId = userId,
            StartedAt = DateTime.UtcNow,
            Status = "InProgress"
        };

        await _repository.AddQuizAttemptAsync(attempt);
        await _repository.SaveChangesAsync();

        return attempt;
    }

}

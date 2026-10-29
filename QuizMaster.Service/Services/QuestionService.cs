
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

    public async Task<List<Question>> GetAllAsync()
    {
        var questions = await _repository.GetAllQuestionsAsync();
        return questions.ToList();
    }

    public Task<Question?> GetByIdAsync(int id)
    {
        return _repository.GetQuestionByIdAsync(id);
    }

    public async Task AddAsync(Question question)
    {
        await _repository.AddQuestionAsync(question);
        await _repository.SaveChangesAsync();
    }

    public async Task<bool> UpdateAsync(Question question)
    {
        var existingQuestion =
            await _repository.GetQuestionByIdAsync(question.Id);

        if (existingQuestion is null)
            return false;

        await _repository.UpdateQuestionAsync(question.Id, question);
        await _repository.SaveChangesAsync();

        return true;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var question = await _repository.GetQuestionByIdAsync(id);

        if (question is null)
            return false;

        await _repository.DeleteQuestionAsync(id);
        await _repository.SaveChangesAsync();

        return true;
    }
}

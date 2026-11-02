
using QuizMaster.Domain.Entities;
using QuizMaster.Service.Interfaces;
using QuizMaster.Service.Interfaces.Repositories;
using QuizMaster.Domain.Interfaces;

namespace QuizMaster.Service.Services;

public class QuestionService : IQuestionService
{
    private readonly IQuestionRepository _repository;
    private readonly IStudentAnswerRepository _studentAnswerRepository;
    public QuestionService(
     IQuestionRepository repository,
     IStudentAnswerRepository studentAnswerRepository)
    {
        _repository = repository;
        _studentAnswerRepository = studentAnswerRepository;
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
        var existingQuestion = await _repository
            .GetQuestionByIdAsync(question.Id);

        if (existingQuestion == null)
            return false;
        if (await _studentAnswerRepository.HasAnswersForQuestionAsync(question.Id))
            return false;

        var existingOptionIds = existingQuestion.AnswerOptions
            .Select(a => a.Id)
            .ToHashSet();

        foreach (var option in question.AnswerOptions)
        {
            if (option.Id != 0 && !existingOptionIds.Contains(option.Id))
                return false;
        }

        await _repository.UpdateQuestionAsync(question.Id, question);
        await _repository.SaveChangesAsync();

        return true;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var question = await _repository.GetQuestionByIdAsync(id);

        if (question is null)
            return false;
        if (await _studentAnswerRepository.HasAnswersForQuestionAsync(id))
            return false;

        await _repository.DeleteQuestionAsync(id);
        await _repository.SaveChangesAsync();

        return true;
    }
}


using QuizMaster.Domain.Entities;
using QuizMaster.Service.Interfaces;
using QuizMaster.Service.Interfaces.Repositories;

namespace QuizMaster.Service.Services;

public class AnswerOptionService : IAnswerOptionService
{
    private readonly IAnswerOptionRepository _repository;
    private readonly IStudentAnswerRepository _studentAnswerRepository;

    public AnswerOptionService(
     IAnswerOptionRepository repository,
     IStudentAnswerRepository studentAnswerRepository)
    {
        _repository = repository;
        _studentAnswerRepository = studentAnswerRepository;
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

    public async Task<bool> AddAsync(AnswerOption answerOption)
    {
        var hasAnswers = await _studentAnswerRepository
            .HasAnswersForQuestionAsync(answerOption.QuestionId);

        if (hasAnswers)
            return false;

        await _repository.AddAnswerOptionAsync(answerOption);
        await _repository.SaveChangesAsync();

        return true;
    }

    public async Task<bool> UpdateAsync(AnswerOption answerOption)
    {
        var existingOption =
            await _repository.GetAnswerOptionByIdAsync(answerOption.Id);

        if (existingOption == null)
            return false;

        if (await _studentAnswerRepository.HasAnswersForQuestionAsync(existingOption.QuestionId))
            return false;


        existingOption.Text = answerOption.Text;
        existingOption.IsCorrect = answerOption.IsCorrect;

        await _repository.SaveChangesAsync();

        return true;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var answerOption =
            await _repository.GetAnswerOptionByIdAsync(id);

        if (answerOption == null)
            return false;
        if (await _studentAnswerRepository.HasAnswersForOptionAsync(id))
            return false;

        await _repository.DeleteAnswerOptionAsync(id);
        await _repository.SaveChangesAsync();

        return true;
    }
}

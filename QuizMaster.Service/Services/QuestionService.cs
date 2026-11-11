
using QuizMaster.Domain.Entities;
using QuizMaster.Domain.Enums;
using QuizMaster.Domain.Interfaces;
using QuizMaster.Service.Interfaces;
using QuizMaster.Service.Interfaces.Repositories;

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
    private bool IsQuestionValid(Question question)
    {
        if (question.AnswerOptions == null ||
            question.AnswerOptions.Count == 0)
            return false;

        var correctAnswers = question.AnswerOptions
            .Count(a => a.IsCorrect);

        if (question.Type == QuestionType.SingleChoice)
            return correctAnswers == 1;

        if (question.Type == QuestionType.MultipleChoice)
            return correctAnswers >= 2;

        if (question.Type == QuestionType.TrueFalse)
            return question.AnswerOptions.Count == 2 &&
                   correctAnswers == 1;

        return false;
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
    public async Task<List<Question>> GetByQuizIdAsync(int quizId)
    {
        var questions = await _repository.GetQuestionsByQuizIdAsync(quizId);

        return questions.ToList();
    }
    public async Task AddAsync(Question question)
    {
        if (!IsQuestionValid(question))
            throw new ArgumentException("Invalid question or answer options.");

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
    public async Task<List<Question>> GetQuestionsByLecturerIdAsync(int lecturerId)
    {
        var questions = await _repository.GetQuestionsByLecturerIdAsync(lecturerId);

        return questions.ToList();
    }
}

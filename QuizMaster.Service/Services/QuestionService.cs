using QuizMaster.Domain.DTOs.Questions;
using QuizMaster.Domain.Entities;
using QuizMaster.Service.Exceptions;
using QuizMaster.Service.Interfaces;
using QuizMaster.Service.Interfaces.Repositories;
using QuizMaster.Service.Rules;

namespace QuizMaster.Service.Services;

public class QuestionService : IQuestionService
{
    private readonly IQuestionRepository _repository;
    private readonly IStudentAnswerRepository _studentAnswerRepository;
    private readonly IQuizService _quizService;

    public QuestionService(
        IQuestionRepository repository,
        IStudentAnswerRepository studentAnswerRepository,
        IQuizService quizService)
    {
        _repository = repository;
        _studentAnswerRepository = studentAnswerRepository;
        _quizService = quizService;
    }

    public Task<Question?> GetByIdAsync(int id)
    {
        return _repository.GetQuestionByIdAsync(id);
    }

    public async Task<List<QuestionResponseDTO>> GetByLecturerAsync(int lecturerId)
    {
        var questions = await _repository.GetQuestionsByLecturerIdAsync(lecturerId);

        return questions.Select(q => ToDto(q)).ToList();
    }

    public async Task<QuestionResponseDTO> GetOwnedAsync(
        int questionId,
        int lecturerId)
    {
        var question = await GetQuestionAsync(questionId);

        await _quizService.GetOwnedQuizAsync(question.QuizId, lecturerId);

        return ToDto(question);
    }

    public async Task<QuestionResponseDTO> CreateAsync(
        int quizId,
        CreateQuestionDTO dto,
        int lecturerId)
    {
        await _quizService.GetEditableQuizAsync(quizId, lecturerId);

        var question = new Question
        {
            QuizId = quizId,
            Text = dto.Text,
            Type = dto.Type,
            Points = dto.Points,
            AnswerOptions = dto.AnswerOptions.Select(a =>
                new AnswerOption
                {
                    Text = a.Text,
                    IsCorrect = a.IsCorrect
                }).ToList()
        };

        if (!QuestionRules.IsValid(question))
            throw new BusinessRuleException(
                "Invalid question or answer options.");

        await _repository.AddQuestionAsync(question);
        await _repository.SaveChangesAsync();

        return ToDto(question);
    }

    public async Task UpdateAsync(
        int questionId,
        UpdateQuestionDTO dto,
        int lecturerId)
    {
        var existing = await GetQuestionAsync(questionId);

        await _quizService.GetEditableQuizAsync(existing.QuizId, lecturerId);

        var updated = new Question
        {
            Id = questionId,
            QuizId = existing.QuizId,
            Text = dto.Text,
            Type = dto.Type,
            Points = dto.Points,
            AnswerOptions = dto.AnswerOptions.Select(a =>
                new AnswerOption
                {
                    Id = a.Id ?? 0,
                    QuestionId = questionId,
                    Text = a.Text,
                    IsCorrect = a.IsCorrect
                }).ToList()
        };

        if (!QuestionRules.IsValid(updated))
            throw new BusinessRuleException(
                "Invalid question or answer options.");

        if (await _studentAnswerRepository.HasAnswersForQuestionAsync(questionId))
            throw new BusinessRuleException(
                "Question cannot be updated because students have already answered it.");

        var existingOptionIds = existing.AnswerOptions
            .Select(a => a.Id)
            .ToHashSet();

        if (updated.AnswerOptions.Any(o =>
                o.Id != 0 && !existingOptionIds.Contains(o.Id)))
            throw new BusinessRuleException(
                "One or more answer option IDs do not belong to this question.");

        await _repository.UpdateQuestionAsync(questionId, updated);
        await _repository.SaveChangesAsync();
    }

    public async Task DeleteAsync(int questionId, int lecturerId)
    {
        var question = await GetQuestionAsync(questionId);

        await _quizService.GetEditableQuizAsync(question.QuizId, lecturerId);

        if (await _studentAnswerRepository.HasAnswersForQuestionAsync(questionId))
            throw new BusinessRuleException(
                "Question cannot be deleted because students have already answered it.");

        await _repository.DeleteQuestionAsync(questionId);
        await _repository.SaveChangesAsync();
    }

    private async Task<Question> GetQuestionAsync(int questionId)
    {
        var question = await _repository.GetQuestionByIdAsync(questionId);

        if (question is null)
            throw new NotFoundException("Question not found.");

        return question;
    }

    private static QuestionResponseDTO ToDto(Question question)
    {
        return new QuestionResponseDTO
        {
            Id = question.Id,
            Text = question.Text,
            Type = question.Type,
            Points = question.Points,
            AnswerOptions = question.AnswerOptions.Select(a =>
                new AnswerOptionResponseDTO
                {
                    Id = a.Id,
                    Text = a.Text
                }).ToList()
        };
    }
}
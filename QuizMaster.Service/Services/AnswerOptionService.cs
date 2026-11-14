using QuizMaster.Domain.DTOs.Questions;
using QuizMaster.Domain.Entities;
using QuizMaster.Service.Exceptions;
using QuizMaster.Service.Interfaces;
using QuizMaster.Service.Interfaces.Repositories;

namespace QuizMaster.Service.Services;

public class AnswerOptionService : IAnswerOptionService
{
    private readonly IAnswerOptionRepository _repository;
    private readonly IQuestionRepository _questionRepository;
    private readonly IStudentAnswerRepository _studentAnswerRepository;
    private readonly IQuizService _quizService;

    public AnswerOptionService(
        IAnswerOptionRepository repository,
        IQuestionRepository questionRepository,
        IStudentAnswerRepository studentAnswerRepository,
        IQuizService quizService)
    {
        _repository = repository;
        _questionRepository = questionRepository;
        _studentAnswerRepository = studentAnswerRepository;
        _quizService = quizService;
    }

    public async Task<List<AnswerOptionDetailsDTO>> GetByQuestionAsync(
        int questionId,
        int lecturerId)
    {
        var question = await GetQuestionAsync(questionId);

        await _quizService.GetOwnedQuizAsync(question.QuizId, lecturerId);

        return question.AnswerOptions.Select(a => ToDto(a)).ToList();
    }

    public async Task<AnswerOptionDetailsDTO> CreateAsync(
        int questionId,
        AnswerOptionDTO dto,
        int lecturerId)
    {
        var question = await GetQuestionAsync(questionId);

        await _quizService.GetEditableQuizAsync(question.QuizId, lecturerId);

        if (await _studentAnswerRepository.HasAnswersForQuestionAsync(questionId))
            throw new BusinessRuleException(
                "Cannot add an answer option because students have already answered this question.");

        var option = new AnswerOption
        {
            QuestionId = questionId,
            Text = dto.Text,
            IsCorrect = dto.IsCorrect
        };

        await _repository.AddAnswerOptionAsync(option);
        await _repository.SaveChangesAsync();

        return ToDto(option);
    }

    public async Task UpdateAsync(
        int answerOptionId,
        AnswerOptionDTO dto,
        int lecturerId)
    {
        var option = await GetOptionAsync(answerOptionId);
        var question = await GetQuestionAsync(option.QuestionId);

        await _quizService.GetEditableQuizAsync(question.QuizId, lecturerId);

        if (await _studentAnswerRepository.HasAnswersForQuestionAsync(option.QuestionId))
            throw new BusinessRuleException(
                "Cannot update this answer option because students have already answered this question.");

        option.Text = dto.Text;
        option.IsCorrect = dto.IsCorrect;

        await _repository.SaveChangesAsync();
    }

    public async Task DeleteAsync(int answerOptionId, int lecturerId)
    {
        var option = await GetOptionAsync(answerOptionId);
        var question = await GetQuestionAsync(option.QuestionId);

        await _quizService.GetEditableQuizAsync(question.QuizId, lecturerId);

        if (await _studentAnswerRepository.HasAnswersForOptionAsync(answerOptionId))
            throw new BusinessRuleException(
                "Cannot delete this answer option because students have already answered it.");

        await _repository.DeleteAnswerOptionAsync(answerOptionId);
        await _repository.SaveChangesAsync();
    }

    private async Task<Question> GetQuestionAsync(int questionId)
    {
        var question = await _questionRepository.GetQuestionByIdAsync(questionId);

        if (question is null)
            throw new NotFoundException("Question not found.");

        return question;
    }

    private async Task<AnswerOption> GetOptionAsync(int answerOptionId)
    {
        var option = await _repository.GetAnswerOptionByIdAsync(answerOptionId);

        if (option is null)
            throw new NotFoundException("Answer option not found.");

        return option;
    }

    private static AnswerOptionDetailsDTO ToDto(AnswerOption option)
    {
        return new AnswerOptionDetailsDTO
        {
            Id = option.Id,
            QuestionId = option.QuestionId,
            Text = option.Text,
            IsCorrect = option.IsCorrect
        };
    }
}
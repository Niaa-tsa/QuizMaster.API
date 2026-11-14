using QuizMaster.Domain.DTOs.Quizzes;
using QuizMaster.Domain.Entities;
using QuizMaster.Service.Exceptions;
using QuizMaster.Service.Interfaces;
using QuizMaster.Service.Interfaces.Repositories;
using QuizMaster.Service.Rules;

namespace QuizMaster.Service.Services;

public class QuizService : IQuizService
{
    private readonly IQuizRepository _repository;
    private readonly IQuizAttemptRepository _attemptRepository;

    public QuizService(
        IQuizRepository repository,
        IQuizAttemptRepository attemptRepository)
    {
        _repository = repository;
        _attemptRepository = attemptRepository;
    }

    public async Task<List<QuizResponseDTO>> GetPublishedAsync()
    {
        var quizzes = await _repository.GetPublishedQuizzesAsync();

        return quizzes.Select(q => ToDto(q)).ToList();
    }

    public async Task<List<QuizResponseDTO>> GetByLecturerAsync(int lecturerId)
    {
        var quizzes = await _repository.GetQuizzesByLecturerIdAsync(lecturerId);

        return quizzes.Select(q => ToDto(q)).ToList();
    }

    public async Task<QuizResponseDTO> GetOwnedAsync(int quizId, int lecturerId)
    {
        var quiz = await GetOwnedQuizAsync(quizId, lecturerId);

        return ToDto(quiz);
    }

    public async Task<QuizResponseDTO> CreateAsync(
        CreateQuizDTO dto,
        int lecturerId)
    {
        var quiz = new Quiz
        {
            Title = dto.Title,
            TimeLimitMinutes = dto.TimeLimitMinutes,
            MaxAttempts = dto.MaxAttempts,
            IsPublished = false,
            LecturerId = lecturerId
        };

        await _repository.AddQuizAsync(quiz);
        await _repository.SaveChangesAsync();

        return ToDto(quiz);
    }

    public async Task<QuizResponseDTO> UpdateAsync(
        int quizId,
        UpdateQuizDTO dto,
        int lecturerId)
    {
        var quiz = await GetEditableQuizAsync(quizId, lecturerId);

        quiz.Title = dto.Title;
        quiz.TimeLimitMinutes = dto.TimeLimitMinutes;
        quiz.MaxAttempts = dto.MaxAttempts;

        await _repository.UpdateQuizAsync(quizId, quiz);
        await _repository.SaveChangesAsync();

        return ToDto(quiz);
    }

    public async Task DeleteAsync(int quizId, int lecturerId)
    {
        var quiz = await GetEditableQuizAsync(quizId, lecturerId);

        await _repository.DeleteQuizAsync(quiz.Id);
        await _repository.SaveChangesAsync();
    }

    public async Task PublishAsync(int quizId, int lecturerId)
    {
        var quiz = await GetOwnedQuizAsync(quizId, lecturerId);

        if (quiz.IsPublished)
            throw new BusinessRuleException("Quiz is already published.");

        if (quiz.Questions == null || !quiz.Questions.Any())
            throw new BusinessRuleException(
                "Quiz must contain at least one question.");

        if (quiz.Questions.Any(q => !QuestionRules.IsValid(q)))
            throw new BusinessRuleException(
                "Every question must have valid answer options for its type.");

        if (await _attemptRepository.HasAttemptsForQuizAsync(quizId))
            throw new BusinessRuleException(
                "Quiz already has student attempts and cannot be published.");

        quiz.IsPublished = true;

        await _repository.UpdateQuizAsync(quizId, quiz);
        await _repository.SaveChangesAsync();
    }

    public async Task CloseAsync(int quizId, int lecturerId)
    {
        var quiz = await GetOwnedQuizAsync(quizId, lecturerId);

        if (!quiz.IsPublished)
            throw new BusinessRuleException("Quiz is already closed.");

        quiz.IsPublished = false;

        await _repository.UpdateQuizAsync(quizId, quiz);
        await _repository.SaveChangesAsync();
    }

    /// <summary>
    /// აბრუნებს ტესტს, თუ ის არსებობს და ეკუთვნის ამ ლექტორს.
    /// სხვა შემთხვევაში ისვრის NotFoundException ან ForbiddenException-ს.
    /// </summary>
    public async Task<Quiz> GetOwnedQuizAsync(int quizId, int lecturerId)
    {
        var quiz = await _repository.GetQuizByIdAsync(quizId);

        if (quiz is null)
            throw new NotFoundException("Quiz not found.");

        if (quiz.LecturerId != lecturerId)
            throw new ForbiddenException(
                "You do not have access to this quiz.");

        return quiz;
    }

    /// <summary>
    /// ამატებს რედაქტირების წესებს: ტესტი არ უნდა იყოს გამოქვეყნებული
    /// და სტუდენტებს მასზე მცდელობა არ უნდა ჰქონდეთ.
    /// </summary>
    public async Task<Quiz> GetEditableQuizAsync(int quizId, int lecturerId)
    {
        var quiz = await GetOwnedQuizAsync(quizId, lecturerId);

        if (quiz.IsPublished)
            throw new BusinessRuleException(
                "Cannot modify a published quiz.");

        if (await _attemptRepository.HasAttemptsForQuizAsync(quizId))
            throw new BusinessRuleException(
                "Cannot modify a quiz that students have already attempted.");

        return quiz;
    }

    private static QuizResponseDTO ToDto(Quiz quiz)
    {
        return new QuizResponseDTO
        {
            Id = quiz.Id,
            Title = quiz.Title,
            TimeLimitMinutes = quiz.TimeLimitMinutes,
            IsPublished = quiz.IsPublished,
            MaxAttempts = quiz.MaxAttempts,
            CreatedAt = quiz.CreatedAt
        };
    }
}
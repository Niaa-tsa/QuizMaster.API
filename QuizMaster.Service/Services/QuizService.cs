
using QuizMaster.Domain.Entities;
using QuizMaster.Domain.Enums;
using QuizMaster.Service.Interfaces;
using QuizMaster.Service.Interfaces.Repositories;

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

        if (existingQuiz is null || existingQuiz.IsPublished)
            return false;

        var hasAttempts =
            await _attemptRepository.HasAttemptsForQuizAsync(quiz.Id);

        if (hasAttempts)
            return false;

        await _repository.UpdateQuizAsync(quiz.Id, quiz);
        await _repository.SaveChangesAsync();

        return true;
    }
    public async Task<bool> DeleteAsync(int id)
    {
        var quiz = await _repository.GetQuizByIdAsync(id);

        if (quiz is null || quiz.IsPublished)
            return false;

        var hasAttempts =
            await _attemptRepository.HasAttemptsForQuizAsync(id);

        if (hasAttempts)
            return false;

        await _repository.DeleteQuizAsync(id);
        await _repository.SaveChangesAsync();

        return true;
    }
    public async Task<bool> HasAttemptsAsync(int quizId)
    {
        return await _attemptRepository.HasAttemptsForQuizAsync(quizId);
    }

    public async Task<bool> PublishAsync(int id)
    {
        var quiz = await _repository.GetQuizByIdAsync(id);

        if (quiz is null || quiz.IsPublished)
            return false;

        if (quiz.Questions == null || !quiz.Questions.Any())
            return false;

        foreach (var question in quiz.Questions)
        {
            if (question.AnswerOptions == null ||
                question.AnswerOptions.Count == 0)
                return false;

            var correctAnswers = question.AnswerOptions
                .Count(a => a.IsCorrect);

            if (question.Type == QuestionType.SingleChoice &&
                correctAnswers != 1)
                return false;

            if (question.Type == QuestionType.MultipleChoice &&
                correctAnswers < 2)
                return false;

            if (question.Type == QuestionType.TrueFalse &&
                (question.AnswerOptions.Count != 2 ||
                 correctAnswers != 1))
                return false;

            if (question.Type != QuestionType.SingleChoice &&
                question.Type != QuestionType.MultipleChoice &&
                question.Type != QuestionType.TrueFalse)
                return false;
        }

        var hasAttempts =
            await _attemptRepository.HasAttemptsForQuizAsync(id);

        if (hasAttempts)
            return false;

        quiz.IsPublished = true;

        await _repository.UpdateQuizAsync(id, quiz);
        await _repository.SaveChangesAsync();

        return true;
    }


    public async Task<bool> CloseAsync(int id)
    {
        var quiz = await _repository.GetQuizByIdAsync(id);
        if (quiz is null || !quiz.IsPublished)
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


    public async Task<List<Quiz>> GetQuizzesByLecturerIdAsync(int lecturerId)
    {
        var quizzes = await _repository.GetQuizzesByLecturerIdAsync(lecturerId);

        return quizzes.ToList();
    }

}

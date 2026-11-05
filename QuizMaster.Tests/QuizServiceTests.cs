using Moq;
using Xunit;
using QuizMaster.Domain.Entities;
using QuizMaster.Service.Services;
using QuizMaster.Service.Interfaces.Repositories;

namespace QuizMaster.Tests;

public class QuizServiceTests
{
    private readonly Mock<IQuizRepository> _quizRepoMock;
    private readonly Mock<IQuizAttemptRepository> _attemptRepoMock;
    private readonly QuizService _service;

    public QuizServiceTests()
    {
        _quizRepoMock = new Mock<IQuizRepository>();
        _attemptRepoMock = new Mock<IQuizAttemptRepository>();
        _service = new QuizService(_quizRepoMock.Object, _attemptRepoMock.Object);
    }

    // 1. გამოქვეყნებული ტესტის რედაქტირება უნდა დაიბლოკოს (Return False)
    [Fact]
    public async Task UpdateAsync_ShouldReturnFalse_WhenQuizIsPublished()
    {
        var quiz = new Quiz { Id = 1, Title = "Updated Quiz", IsPublished = true };

        _quizRepoMock.Setup(r => r.GetQuizByIdAsync(quiz.Id))
            .ReturnsAsync(quiz);

        var result = await _service.UpdateAsync(quiz);

        Assert.False(result);
    }

    // 2. ტესტის წაშლა უნდა დაიბლოკოს, თუ მასზე სტუდენტის მცდელობები არსებობს
    [Fact]
    public async Task DeleteAsync_ShouldReturnFalse_WhenQuizHasAttempts()
    {
        int quizId = 1;
        var quiz = new Quiz { Id = quizId, IsPublished = false };

        _quizRepoMock.Setup(r => r.GetQuizByIdAsync(quizId))
            .ReturnsAsync(quiz);

        _attemptRepoMock.Setup(r => r.HasAttemptsForQuizAsync(quizId))
            .ReturnsAsync(true);

        var result = await _service.DeleteAsync(quizId);

        Assert.False(result);
    }

    // 3. ტესტი ვერ გამოქვეყნდება, თუ მასში კითხვები არ არის დამატებული
    [Fact]
    public async Task PublishAsync_ShouldReturnFalse_WhenQuizHasNoQuestions()
    {
        int quizId = 1;
        var quiz = new Quiz
        {
            Id = quizId,
            IsPublished = false,
            Questions = new List<Question>() // ცარიელი კითხვების სია
        };

        _quizRepoMock.Setup(r => r.GetQuizByIdAsync(quizId))
            .ReturnsAsync(quiz);

        var result = await _service.PublishAsync(quizId);

        Assert.False(result);
    }
}
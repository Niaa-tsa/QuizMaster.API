using Moq;
using Xunit;
using QuizMaster.Domain.DTOs.Quizzes;
using QuizMaster.Domain.Entities;
using QuizMaster.Service.Exceptions;
using QuizMaster.Service.Interfaces.Repositories;
using QuizMaster.Service.Services;

namespace QuizMaster.Tests;

public class QuizServiceTests
{
    private const int LecturerId = 7;

    private readonly Mock<IQuizRepository> _quizRepoMock;
    private readonly Mock<IQuizAttemptRepository> _attemptRepoMock;
    private readonly QuizService _service;

    public QuizServiceTests()
    {
        _quizRepoMock = new Mock<IQuizRepository>();
        _attemptRepoMock = new Mock<IQuizAttemptRepository>();
        _service = new QuizService(_quizRepoMock.Object, _attemptRepoMock.Object);
    }

    // 1. გამოქვეყნებული ტესტის რედაქტირება უნდა დაიბლოკოს
    [Fact]
    public async Task UpdateAsync_ShouldThrowBusinessRule_WhenQuizIsPublished()
    {
        var quiz = new Quiz { Id = 1, LecturerId = LecturerId, IsPublished = true };

        _quizRepoMock.Setup(r => r.GetQuizByIdAsync(quiz.Id))
            .ReturnsAsync(quiz);

        var dto = new UpdateQuizDTO { Title = "Updated Quiz", TimeLimitMinutes = 10 };

        await Assert.ThrowsAsync<BusinessRuleException>(
            () => _service.UpdateAsync(quiz.Id, dto, LecturerId));
    }

    // 2. ტესტის წაშლა უნდა დაიბლოკოს, თუ მასზე სტუდენტის მცდელობები არსებობს
    [Fact]
    public async Task DeleteAsync_ShouldThrowBusinessRule_WhenQuizHasAttempts()
    {
        int quizId = 1;
        var quiz = new Quiz { Id = quizId, LecturerId = LecturerId, IsPublished = false };

        _quizRepoMock.Setup(r => r.GetQuizByIdAsync(quizId))
            .ReturnsAsync(quiz);

        _attemptRepoMock.Setup(r => r.HasAttemptsForQuizAsync(quizId))
            .ReturnsAsync(true);

        await Assert.ThrowsAsync<BusinessRuleException>(
            () => _service.DeleteAsync(quizId, LecturerId));
    }

    // 3. ტესტი ვერ გამოქვეყნდება, თუ მასში კითხვები არ არის დამატებული
    [Fact]
    public async Task PublishAsync_ShouldThrowBusinessRule_WhenQuizHasNoQuestions()
    {
        int quizId = 1;
        var quiz = new Quiz
        {
            Id = quizId,
            LecturerId = LecturerId,
            IsPublished = false,
            Questions = new List<Question>()
        };

        _quizRepoMock.Setup(r => r.GetQuizByIdAsync(quizId))
            .ReturnsAsync(quiz);

        await Assert.ThrowsAsync<BusinessRuleException>(
            () => _service.PublishAsync(quizId, LecturerId));
    }

    // 4. სხვა ლექტორის ტესტზე წვდომა აკრძალულია (ownership)
    [Fact]
    public async Task GetOwnedAsync_ShouldThrowForbidden_WhenQuizBelongsToAnotherLecturer()
    {
        int quizId = 1;
        var quiz = new Quiz { Id = quizId, LecturerId = 999 };

        _quizRepoMock.Setup(r => r.GetQuizByIdAsync(quizId))
            .ReturnsAsync(quiz);

        await Assert.ThrowsAsync<ForbiddenException>(
            () => _service.GetOwnedAsync(quizId, LecturerId));
    }

    // 5. არარსებული ტესტი -> NotFoundException
    [Fact]
    public async Task GetOwnedAsync_ShouldThrowNotFound_WhenQuizDoesNotExist()
    {
        _quizRepoMock.Setup(r => r.GetQuizByIdAsync(It.IsAny<int>()))
            .ReturnsAsync((Quiz?)null);

        await Assert.ThrowsAsync<NotFoundException>(
            () => _service.GetOwnedAsync(123, LecturerId));
    }
}
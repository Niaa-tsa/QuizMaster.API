using Moq;
using Xunit;
using QuizMaster.Domain.Entities;
using QuizMaster.Domain.Enums;
using QuizMaster.Service.Services;
using QuizMaster.Service.Interfaces.Repositories;

namespace QuizMaster.Tests;

public class QuizAttemptServiceTests
{
    private readonly Mock<IQuizAttemptRepository> _attemptRepoMock;
    private readonly Mock<IQuizRepository> _quizRepoMock;
    private readonly Mock<IStudentAnswerRepository> _studentAnswerRepoMock;
    private readonly Mock<IQuizResultRepository> _quizResultRepoMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly QuizAttemptService _service;

    public QuizAttemptServiceTests()
    {
        _attemptRepoMock = new Mock<IQuizAttemptRepository>();
        _quizRepoMock = new Mock<IQuizRepository>();
        _studentAnswerRepoMock = new Mock<IStudentAnswerRepository>();
        _quizResultRepoMock = new Mock<IQuizResultRepository>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();

        _service = new QuizAttemptService(
            _attemptRepoMock.Object,
            _quizRepoMock.Object,
            _studentAnswerRepoMock.Object,
            _quizResultRepoMock.Object,
            _unitOfWorkMock.Object
        );
    }

    // 1. შემოწმება, რომ არარსებული მცდელობის დასრულებისას აბრუნებს null-ს
    [Fact]
    public async Task FinishAttemptAsync_ShouldReturnNull_WhenAttemptNotFoundOrNotInProgress()
    {
        int attemptId = 1;
        int userId = 1;

        _attemptRepoMock.Setup(r => r.GetStudentAttemptAsync(attemptId, userId))
            .ReturnsAsync((QuizAttempt?)null);

        var result = await _service.FinishAttemptAsync(attemptId, userId);

        Assert.Null(result);
    }

    // 2. ქულების ავტომატური დათვლა Single Choice კითხვაზე
    [Fact]
    public async Task FinishAttemptAsync_ShouldCalculateCorrectScore_ForSingleChoice()
    {
        int attemptId = 1;
        int userId = 1;
        int questionId = 10;
        int correctOptionId = 100;

        var quiz = new Quiz
        {
            Id = 1,
            TimeLimitMinutes = 30,
            Questions = new List<Question>
            {
                new Question
                {
                    Id = questionId,
                    Points = 10,
                    Type = QuestionType.SingleChoice,
                    AnswerOptions = new List<AnswerOption>
                    {
                        new AnswerOption { Id = correctOptionId, IsCorrect = true },
                        new AnswerOption { Id = 101, IsCorrect = false }
                    }
                }
            }
        };

        var attempt = new QuizAttempt
        {
            Id = attemptId,
            UserId = userId,
            QuizId = 1,
            Status = "InProgress",
            StartedAt = DateTime.UtcNow.AddMinutes(-5),
            Quiz = quiz
        };

        _attemptRepoMock.Setup(r => r.GetStudentAttemptAsync(attemptId, userId))
            .ReturnsAsync(attempt);

        _studentAnswerRepoMock.Setup(r => r.GetAnswersByAttemptAndQuestionAsync(attemptId, questionId))
            .ReturnsAsync(new List<StudentAnswer>
            {
                new StudentAnswer { AttemptId = attemptId, QuestionId = questionId, AnswerOptionId = correctOptionId }
            });

        var result = await _service.FinishAttemptAsync(attemptId, userId);

        Assert.NotNull(result);
        Assert.Equal(10, result.Score);
        Assert.Equal(10, result.MaxScore);
    }

    // 3. შემოწმება, რომ არასწორი პასუხის მონიშვნისას ქულა არ იწერება (Score = 0)
    [Fact]
    public async Task FinishAttemptAsync_ShouldReturnZeroScore_WhenStudentSelectsWrongAnswer()
    {
        int attemptId = 1;
        int userId = 1;
        int questionId = 10;
        int correctOptionId = 100;
        int wrongOptionId = 101;

        var quiz = new Quiz
        {
            Id = 1,
            TimeLimitMinutes = 30,
            Questions = new List<Question>
            {
                new Question
                {
                    Id = questionId,
                    Points = 10,
                    Type = QuestionType.SingleChoice,
                    AnswerOptions = new List<AnswerOption>
                    {
                        new AnswerOption { Id = correctOptionId, IsCorrect = true },
                        new AnswerOption { Id = wrongOptionId, IsCorrect = false }
                    }
                }
            }
        };

        var attempt = new QuizAttempt
        {
            Id = attemptId,
            UserId = userId,
            QuizId = 1,
            Status = "InProgress",
            StartedAt = DateTime.UtcNow.AddMinutes(-5),
            Quiz = quiz
        };

        _attemptRepoMock.Setup(r => r.GetStudentAttemptAsync(attemptId, userId))
            .ReturnsAsync(attempt);

        // სტუდენტმა აირჩია არასწორი პასუხი
        _studentAnswerRepoMock.Setup(r => r.GetAnswersByAttemptAndQuestionAsync(attemptId, questionId))
            .ReturnsAsync(new List<StudentAnswer>
            {
                new StudentAnswer { AttemptId = attemptId, QuestionId = questionId, AnswerOptionId = wrongOptionId }
            });

        var result = await _service.FinishAttemptAsync(attemptId, userId);

        Assert.NotNull(result);
        Assert.Equal(0, result.Score);
        Assert.Equal(10, result.MaxScore);
    }

    // 4. Multiple Choice კითხვაზე ქულის დათვლა (მხოლოდ ყველა სწორი პასუხის მონიშვნისას)
    [Fact]
    public async Task FinishAttemptAsync_ShouldGiveScore_ForMultipleChoice_OnlyWhenAllCorrectOptionsSelected()
    {
        int attemptId = 1;
        int userId = 1;
        int questionId = 20;

        var quiz = new Quiz
        {
            Id = 1,
            TimeLimitMinutes = 30,
            Questions = new List<Question>
            {
                new Question
                {
                    Id = questionId,
                    Points = 15,
                    Type = QuestionType.MultipleChoice,
                    AnswerOptions = new List<AnswerOption>
                    {
                        new AnswerOption { Id = 201, IsCorrect = true },
                        new AnswerOption { Id = 202, IsCorrect = true },
                        new AnswerOption { Id = 203, IsCorrect = false }
                    }
                }
            }
        };

        var attempt = new QuizAttempt
        {
            Id = attemptId,
            UserId = userId,
            QuizId = 1,
            Status = "InProgress",
            StartedAt = DateTime.UtcNow.AddMinutes(-5),
            Quiz = quiz
        };

        _attemptRepoMock.Setup(r => r.GetStudentAttemptAsync(attemptId, userId))
            .ReturnsAsync(attempt);

        // სტუდენტმა მონიშნა ორივე სწორი პასუხი
        _studentAnswerRepoMock.Setup(r => r.GetAnswersByAttemptAndQuestionAsync(attemptId, questionId))
            .ReturnsAsync(new List<StudentAnswer>
            {
                new StudentAnswer { AttemptId = attemptId, QuestionId = questionId, AnswerOptionId = 201 },
                new StudentAnswer { AttemptId = attemptId, QuestionId = questionId, AnswerOptionId = 202 }
            });

        var result = await _service.FinishAttemptAsync(attemptId, userId);

        Assert.NotNull(result);
        Assert.Equal(15, result.Score);
    }

    // 5. შემოწმება, რომ ტესტის დაწყება შეუძლებელია, თუ ტესტი არ არსებობს ან არაა გამოქვეყნებული
    [Fact]
    public async Task StartAsync_ShouldReturnNull_WhenQuizIsNotPublishedOrNull()
    {
        int quizId = 1;
        int userId = 1;

        _quizRepoMock.Setup(r => r.GetQuizByIdAsync(quizId))
            .ReturnsAsync((Quiz?)null);

        var result = await _service.StartAsync(quizId, userId);

        Assert.Null(result);
    }

    // 6. შემოწმება, რომ სტუდენტს არ შეუძლია ერთსა და იმავე ტესტზე მეორე მცდელობის დაწყება
    [Fact]
    public async Task StartAsync_ShouldReturnNull_WhenStudentAlreadyHasAttemptForQuiz()
    {
        int quizId = 1;
        int userId = 1;

        var quiz = new Quiz { Id = quizId, IsPublished = true };

        _quizRepoMock.Setup(r => r.GetQuizByIdAsync(quizId))
            .ReturnsAsync(quiz);

        _attemptRepoMock.Setup(r => r.GetStudentAttemptsAsync(userId))
            .ReturnsAsync(new List<QuizAttempt>
            {
                new QuizAttempt { QuizId = quizId, UserId = userId, Status = "Finished" }
            });

        var result = await _service.StartAsync(quizId, userId);

        Assert.Null(result);
    }
}
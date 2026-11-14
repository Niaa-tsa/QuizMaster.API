using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuizMaster.Domain.DTOs.Attempts;
using QuizMaster.Domain.DTOs.Questions;
using QuizMaster.Domain.DTOs.Quizzes;
using QuizMaster.Service.Interfaces;
using System.Security.Claims;
namespace QuizMaster.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Student")]
public class StudentQuizzesController : ControllerBase
{
    private readonly IQuizService _quizService;
    private readonly IQuizAttemptService _attemptService;
    private readonly ILogger<StudentQuizzesController> _logger;

    public StudentQuizzesController(
        IQuizService quizService,
        IQuizAttemptService attemptService,
        ILogger<StudentQuizzesController> logger)
    {
        _quizService = quizService;
        _attemptService = attemptService;
        _logger = logger;
    }
    /// <summary>
    /// აბრუნებს ყველა გამოქვეყნებული ტესტის სიას სტუდენტებისთვის.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetPublishedQuizzes()
    {
        var quizzes = await _quizService.GetPublishedAsync();

        var response = quizzes.Select(quiz => new QuizResponseDTO
        {
            Id = quiz.Id,
            Title = quiz.Title,
            TimeLimitMinutes = quiz.TimeLimitMinutes,
            IsPublished = quiz.IsPublished,
            MaxAttempts = quiz.MaxAttempts,
            CreatedAt = quiz.CreatedAt
        }).ToList();

        return Ok(response);
    }
    /// <summary>
    /// იწყებს ახალ მცდელობას მითითებულ ტესტზე.
    /// </summary>
    /// <param name="quizId">ტესტის ID</param>
    [HttpPost("{quizId:int}/start")]
    public async Task<IActionResult> StartQuiz(int quizId)
    {
        var userIdValue = User.FindFirstValue(
            ClaimTypes.NameIdentifier);

        if (!int.TryParse(userIdValue, out var userId))
        {
            return Problem(statusCode: 401, title: "User ID was not found in the token.");
        }

        var attempt = await _attemptService.StartAsync(
            quizId, userId);

        if (attempt is null)
        {
            _logger.LogWarning(
                "User {UserId} could not start quiz {QuizId}",
                userId, quizId);

            return Problem(statusCode: 409, title: "Quiz is unavailable or the attempt limit has been reached.");
        }

        _logger.LogInformation(
            "User {UserId} started attempt {AttemptId} for quiz {QuizId}",
            userId, attempt.Id, quizId);

        return Ok(new
        {
            attempt.Id,
            attempt.QuizId,
            attempt.StartedAt,
            attempt.Status
        });
    }
    /// <summary>
    /// აბრუნებს მიმდინარე მცდელობის კითხვებს (სწორი პასუხების გარეშე).
    /// </summary>
    /// <param name="attemptId">მცდელობის ID</param>
    [HttpGet("attempt/{attemptId:int}/questions")]
    public async Task<IActionResult> GetAttemptQuestions(int attemptId)
    {
        var userIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (!int.TryParse(userIdValue, out var userId))
            return Unauthorized();

        var attempt = await _attemptService.GetStudentAttemptAsync(
            attemptId, userId);

        if (attempt == null)
            return Problem(statusCode: 404, title: "Attempt not found.");

        if (attempt.Status != "InProgress")
            return Problem(statusCode: 400, title: "This attempt is already finished.");

        if (attempt.Quiz == null)
            return Problem(statusCode: 404, title: "Quiz not found.");

        var elapsedTime = DateTime.UtcNow - attempt.StartedAt;

        var timeLimit = TimeSpan.FromMinutes(
            attempt.Quiz.TimeLimitMinutes);

        if (elapsedTime >= timeLimit)
        {
            var result = await _attemptService.FinishAttemptAsync(
                attemptId, userId);

            if (result == null)
            {
                return Problem(statusCode: 400, title: "Quiz could not be completed.");
            }

            return Ok(new
            {
                message = "Time is up. Quiz completed automatically.",
                result.Score,
                result.MaxScore,
                Percentage = result.MaxScore > 0
                    ? Math.Round(result.Score / result.MaxScore * 100, 2)
                    : 0
            });
        }
        var response = attempt.Quiz.Questions
     .Select(q => new StudentQuestionDTO
     {
         Id = q.Id,
         Text = q.Text,
         Type = q.Type,
         Points = q.Points,
         AnswerOptions = q.AnswerOptions.Select(a =>
             new StudentAnswerOptionDTO
             {
                 Id = a.Id,
                 Text = a.Text
             }).ToList()
     }).ToList();

        return Ok(response);
    }

    /// <summary>
    /// ასრულებს ტესტის მცდელობას და ითვლის საბოლოო ქულას.
    /// </summary>
    /// <param name="attemptId">მცდელობის ID</param>
    [HttpPost("attempt/{attemptId:int}/finish")]
    public async Task<IActionResult> FinishQuiz(int attemptId)
    {
        var userIdValue = User.FindFirstValue(
            ClaimTypes.NameIdentifier);

        if (!int.TryParse(userIdValue, out var userId))
            return Unauthorized();

        var result = await _attemptService.FinishAttemptAsync(
            attemptId, userId);

        if (result == null)
        {
            return Problem(statusCode: 400, title: "Attempt not found or already finished.");
        }

        _logger.LogInformation(
     "User {UserId} finished attempt {AttemptId}: {Score}/{MaxScore}",
     userId, attemptId, result.Score, result.MaxScore);

        return Ok(new
        {
            message = "Quiz completed successfully.",
            result.Score,
            result.MaxScore,
            Percentage = result.MaxScore > 0
                ? Math.Round(result.Score / result.MaxScore * 100, 2)
                : 0
        });
    }
    /// <summary>
    /// აბრუნებს ავტორიზებული სტუდენტის მცდელობების ისტორიას.
    /// </summary>
    [HttpGet("history")]
    public async Task<IActionResult> GetHistory()
    {
        var userIdValue = User.FindFirstValue(
            ClaimTypes.NameIdentifier);

        if (!int.TryParse(userIdValue, out var userId))
            return Unauthorized();

        var attempts = await _attemptService.GetStudentAttemptsAsync(userId);

        var response = attempts.Select(a => new StudentAttemptHistoryDTO
        {
            AttemptId = a.Id,
            QuizId = a.QuizId,
            QuizTitle = a.Quiz.Title,
            StartedAt = a.StartedAt,
            FinishedAt = a.FinishedAt,
            Status = a.Status,
            Score = a.QuizResult?.Score,
            MaxScore = a.QuizResult?.MaxScore
        }).ToList();

        return Ok(response);
    }
}
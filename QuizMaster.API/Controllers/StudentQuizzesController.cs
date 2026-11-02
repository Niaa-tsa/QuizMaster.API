
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
    public StudentQuizzesController(
    IQuizService quizService,
    IQuizAttemptService attemptService)
    {
        _quizService = quizService;
        _attemptService = attemptService;
    }

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
            CreatedAt = quiz.CreatedAt
        }).ToList();

        return Ok(response);
    }

    [HttpPost("{quizId:int}/start")]
    public async Task<IActionResult> StartQuiz(int quizId)
    {
        var userIdValue = User.FindFirstValue(
            ClaimTypes.NameIdentifier);

        if (!int.TryParse(userIdValue, out var userId))
        {
            return Unauthorized(new
            {
                message = "User ID was not found in the token."
            });
        }

        var attempt = await _attemptService.StartAsync(
            quizId, userId);

        if (attempt is null)
        {
            return Conflict(new
            {
                message = "Quiz is unavailable, already started, or already completed."
            });
        }

        return Ok(new
        {
            attempt.Id,
            attempt.QuizId,
            attempt.StartedAt,
            attempt.Status
        });
    }
    [HttpGet("attempt/{attemptId:int}/questions")]
    public async Task<IActionResult> GetAttemptQuestions(int attemptId)
    {
        var userIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (!int.TryParse(userIdValue, out var userId))
            return Unauthorized();

        var attempt = await _attemptService.GetStudentAttemptAsync(
            attemptId, userId);

        if (attempt == null)
            return NotFound("Attempt not found.");

        if (attempt.Status != "InProgress")
            return BadRequest("This attempt is already finished.");

        if (attempt.Quiz == null)
            return NotFound("Quiz not found.");

        var elapsedTime = DateTime.UtcNow - attempt.StartedAt;

        var timeLimit = TimeSpan.FromMinutes(
            attempt.Quiz.TimeLimitMinutes);

        if (elapsedTime >= timeLimit)
        {
            var result = await _attemptService.FinishAttemptAsync(
                attemptId, userId);

            if (result == null)
            {
                return BadRequest("Quiz could not be completed.");
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
            return BadRequest(new
            {
                message = "Attempt not found or already finished."
            });
        }

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


using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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
            return NotFound(new
            {
                message = "Quiz not found or not published."
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

}

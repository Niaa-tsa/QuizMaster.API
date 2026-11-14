using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuizMaster.Service.Interfaces;
using System.Security.Claims;

namespace QuizMaster.API.Controllers;

[ApiController]
[Route("api/quizzes/{quizId:int}/statistics")]
[Authorize(Roles = "Lecturer")]
public class QuizStatisticsController : ControllerBase
{
    private readonly IQuizAttemptService _attemptService;

    public QuizStatisticsController(IQuizAttemptService attemptService)
    {
        _attemptService = attemptService;
    }

    /// <summary>
    /// აბრუნებს ტესტის სტატისტიკას.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetStatistics(int quizId)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (!int.TryParse(userId, out int lecturerId))
            return Unauthorized();

        var statistics = await _attemptService
            .GetQuizStatisticsAsync(quizId, lecturerId);

        if (statistics is null)
            return Problem(statusCode: 404, title: "Quiz not found or access denied.");

        return Ok(statistics);
    }
}
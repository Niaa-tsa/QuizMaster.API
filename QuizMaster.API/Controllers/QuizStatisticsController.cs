using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuizMaster.Service.Interfaces;

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

    [HttpGet]
    public async Task<IActionResult> GetStatistics(int quizId)
    {
        var statistics = await _attemptService
            .GetQuizStatisticsAsync(quizId);

        if (statistics is null)
            return NotFound(new { message = "Quiz not found." });

        return Ok(statistics);
    }
}
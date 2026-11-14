using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuizMaster.Service.Interfaces;

namespace QuizMaster.API.Controllers;

[ApiController]
[Route("api/quizzes/{quizId:int}/leaderboard")]
[Authorize]
public class LeaderboardController : ControllerBase
{
    private readonly IQuizAttemptService _attemptService;

    public LeaderboardController(IQuizAttemptService attemptService)
    {
        _attemptService = attemptService;
    }
    /// <summary>
    /// აბრუნებს ტესტის ლიდერბორდს (საუკეთესო შედეგების რეიტინგს).
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetLeaderboard(
        int quizId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
    {
        if (page < 1 || pageSize < 1 || pageSize > 100)
            return Problem(statusCode: 400, title: "Invalid page or pageSize.");

        var leaderboard = await _attemptService
            .GetLeaderboardAsync(quizId, page, pageSize);

        if (leaderboard is null)
            return Problem(statusCode: 404, title: "Quiz not found.");

        return Ok(leaderboard);
    }
}
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuizMaster.Domain.DTOs.Attempts;
using QuizMaster.Service.Interfaces;
using System.Security.Claims;

namespace QuizMaster.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Student")]
public class StudentAnswersController : ControllerBase
{
    private readonly IStudentAnswerService _studentAnswerService;

    public StudentAnswersController(
        IStudentAnswerService studentAnswerService)
    {
        _studentAnswerService = studentAnswerService;
    }

    [HttpPost("attempt/{attemptId:int}/submit")]
    public async Task<IActionResult> SubmitAnswer(
        int attemptId,
        [FromBody] SubmitAnswerDTO dto)
    {
        var userIdValue = User.FindFirstValue(
            ClaimTypes.NameIdentifier);

        if (!int.TryParse(userIdValue, out var userId))
            return Unauthorized();

        var result = await _studentAnswerService.SubmitAnswerAsync(
            attemptId,
            userId,
            dto);

        if (!result)
        {
            return BadRequest(new
            {
                message = "Invalid attempt, question or answer options."
            });
        }

        return Ok(new
        {
            message = "Answer submitted successfully."
        });
    }
}
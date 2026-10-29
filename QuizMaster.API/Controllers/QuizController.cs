
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuizMaster.Domain.DTOs.Quizzes;
using QuizMaster.Domain.Entities;
using QuizMaster.Service.Interfaces;

namespace QuizMaster.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Lecturer")]
public class QuizController : ControllerBase
{
    private readonly IQuizService _quizService;

    public QuizController(IQuizService quizService)
    {
        _quizService = quizService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var quizzes = await _quizService.GetAllAsync();

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

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var quiz = await _quizService.GetByIdAsync(id);

        if (quiz is null)
        {
            return NotFound(new { message = "Quiz not found." });
        }

        var response = new QuizResponseDTO
        {
            Id = quiz.Id,
            Title = quiz.Title,
            TimeLimitMinutes = quiz.TimeLimitMinutes,
            IsPublished = quiz.IsPublished,
            CreatedAt = quiz.CreatedAt
        };

        return Ok(response);
    }

    [HttpPost]
    public async Task<IActionResult> Create(
        [FromBody] CreateQuizDTO request)
    {
        var quiz = new Quiz
        {
            Title = request.Title,
            TimeLimitMinutes = request.TimeLimitMinutes,
            IsPublished = false
        };

        await _quizService.AddAsync(quiz);

        var response = new QuizResponseDTO
        {
            Id = quiz.Id,
            Title = quiz.Title,
            TimeLimitMinutes = quiz.TimeLimitMinutes,
            IsPublished = quiz.IsPublished,
            CreatedAt = quiz.CreatedAt
        };

        return CreatedAtAction(
            nameof(GetById),
            new { id = quiz.Id },
            response);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(
        int id,
        [FromBody] UpdateQuizDTO request)
    {
        var quiz = await _quizService.GetByIdAsync(id);

        if (quiz is null)
        {
            return NotFound(new { message = "Quiz not found." });
        }

        quiz.Title = request.Title;
        quiz.TimeLimitMinutes = request.TimeLimitMinutes;

        var updated = await _quizService.UpdateAsync(quiz);

        if (!updated)
        {
            return NotFound(new { message = "Quiz not found." });
        }

        var response = new QuizResponseDTO
        {
            Id = quiz.Id,
            Title = quiz.Title,
            TimeLimitMinutes = quiz.TimeLimitMinutes,
            IsPublished = quiz.IsPublished,
            CreatedAt = quiz.CreatedAt
        };

        return Ok(response);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var deleted = await _quizService.DeleteAsync(id);

        if (!deleted)
        {
            return NotFound(new { message = "Quiz not found." });
        }

        return NoContent();
    }

    [HttpPatch("{id:int}/publish")]
    public async Task<IActionResult> Publish(int id)
    {
        var published = await _quizService.PublishAsync(id);

        if (!published)
        {
            return NotFound(new { message = "Quiz not found." });
        }

        return Ok(new { message = "Quiz published successfully." });
    }

    [HttpPatch("{id:int}/close")]
    public async Task<IActionResult> Close(int id)
    {
        var closed = await _quizService.CloseAsync(id);

        if (!closed)
        {
            return NotFound(new { message = "Quiz not found." });
        }

        return Ok(new { message = "Quiz closed successfully." });
    }

}

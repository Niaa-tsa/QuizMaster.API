
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuizMaster.Domain.DTOs.Quizzes;
using QuizMaster.Domain.Entities;
using QuizMaster.Service.Interfaces;
using System.Security.Claims;

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

    /// <summary>
    /// აბრუნებს ავტორიზებული ლექტორის მიერ შექმნილ ყველა ტესტს.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (!int.TryParse(userId, out int lecturerId))
            return Unauthorized();

        var quizzes = await _quizService.GetQuizzesByLecturerIdAsync(lecturerId);

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


    /// <summary>
    /// აბრუნებს კონკრეტულ ტესტს ID-ის მიხედვით.
    /// წვდომა აქვს მხოლოდ ტესტის შემქმნელ ლექტორს.
    /// </summary>
    /// <param name="id">ტესტის ID</param>
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var quiz = await _quizService.GetByIdAsync(id);

        if (quiz is null)
            return NotFound(new { message = "Quiz not found." });

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (!int.TryParse(userId, out int lecturerId))
            return Unauthorized();

        if (quiz.LecturerId != lecturerId)
            return Forbid();

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

    /// <summary>
    /// ქმნის ახალ ტესტს.
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "Lecturer")]
    public async Task<IActionResult> Create(
        [FromBody] CreateQuizDTO request)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (!int.TryParse(userId, out int lecturerId))
            return Unauthorized();

        var quiz = new Quiz
        {
            Title = request.Title,
            TimeLimitMinutes = request.TimeLimitMinutes,
            IsPublished = false,
            MaxAttempts = request.MaxAttempts,
            LecturerId = lecturerId
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


    /// <summary>
    /// ანახლებს არსებული ტესტის მონაცემებს.
    /// ტესტის შეცვლა შეუძლია მხოლოდ მის შემქმნელ ლექტორს,
    /// თუ ტესტი გამოქვეყნებული არ არის.
    /// </summary>
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(
        int id,
        [FromBody] UpdateQuizDTO request)
    {
        var quiz = await _quizService.GetByIdAsync(id);

        if (quiz is null)
            return NotFound(new { message = "Quiz not found." });

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (!int.TryParse(userId, out int lecturerId))
            return Unauthorized();

        if (quiz.LecturerId != lecturerId)
            return Forbid();

        if (quiz.IsPublished)
        {
            return BadRequest(new
            {
                message = "Published quizzes cannot be updated."
            });
        }

        quiz.Title = request.Title;
        quiz.TimeLimitMinutes = request.TimeLimitMinutes;
        quiz.MaxAttempts = request.MaxAttempts;
        var updated = await _quizService.UpdateAsync(quiz);

        if (!updated)
        {
            return BadRequest(new
            {
                message = "Quiz cannot be updated because it is published or has student attempts."
            });
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


    /// <summary>
    /// შლის ტესტს მხოლოდ მისი შემქმნელი ლექტორი,
    /// თუ ტესტი გამოქვეყნებული არ არის და არ აქვს მცდელობები.
    /// </summary>
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var quiz = await _quizService.GetByIdAsync(id);

        if (quiz is null)
            return NotFound(new { message = "Quiz not found." });

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (!int.TryParse(userId, out int lecturerId))
            return Unauthorized();

        if (quiz.LecturerId != lecturerId)
            return Forbid();

        if (quiz.IsPublished)
            return BadRequest(new
            {
                message = "Published quizzes cannot be deleted."
            });

        var deleted = await _quizService.DeleteAsync(id);

        if (!deleted)
            return BadRequest(new
            {
                message = "Quiz has student attempts and cannot be deleted."
            });

        return NoContent();
    }


    /// <summary>
    /// აქვეყნებს ტესტს სტუდენტებისთვის ხელმისაწვდომად.
    /// გამოქვეყნება შეუძლია მხოლოდ ტესტის შემქმნელ ლექტორს.
    /// </summary>
    [HttpPatch("{id:int}/publish")]
    public async Task<IActionResult> Publish(int id)
    {
        var quiz = await _quizService.GetByIdAsync(id);

        if (quiz is null)
            return NotFound(new { message = "Quiz not found." });

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (!int.TryParse(userId, out int lecturerId))
            return Unauthorized();

        if (quiz.LecturerId != lecturerId)
            return Forbid();

        if (quiz.IsPublished)
            return BadRequest(new
            {
                message = "Quiz is already published."
            });

        if (quiz.Questions == null || !quiz.Questions.Any())
            return BadRequest(new
            {
                message = "Quiz must contain at least one question."
            });

        var published = await _quizService.PublishAsync(id);

        if (!published)
            return BadRequest(new
            {
                message = "Quiz could not be published."
            });

        return Ok(new { message = "Quiz published successfully." });
    }


    /// <summary>
    /// ხურავს გამოქვეყნებულ ტესტს.
    /// დახურვა შეუძლია მხოლოდ ტესტის შემქმნელ ლექტორს.
    /// </summary>
    [HttpPatch("{id:int}/close")]
    public async Task<IActionResult> Close(int id)
    {
        var quiz = await _quizService.GetByIdAsync(id);

        if (quiz is null)
            return NotFound(new { message = "Quiz not found." });

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (!int.TryParse(userId, out int lecturerId))
            return Unauthorized();

        if (quiz.LecturerId != lecturerId)
            return Forbid();

        if (!quiz.IsPublished)
            return BadRequest(new
            {
                message = "Quiz is already closed."
            });

        var closed = await _quizService.CloseAsync(id);

        if (!closed)
            return BadRequest(new
            {
                message = "Quiz could not be closed."
            });

        return Ok(new { message = "Quiz closed successfully." });
    }

    /// <summary>
    /// აბრუნებს კონკრეტული ლექტორის მიერ შექმნილ ყველა ტესტს.
    /// </summary>
    [HttpGet("my-quizzes")]
    [Authorize(Roles = "Lecturer")]
    public async Task<IActionResult> GetMyQuizzes()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        if (!int.TryParse(userId, out int lecturerId))
            return Unauthorized();

        var quizzes = await _quizService.GetQuizzesByLecturerIdAsync(lecturerId);


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

}

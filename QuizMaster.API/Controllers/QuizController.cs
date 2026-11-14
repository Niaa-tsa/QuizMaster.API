using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuizMaster.API.Extensions;
using QuizMaster.Domain.DTOs.Quizzes;
using QuizMaster.Service.Interfaces;

namespace QuizMaster.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Lecturer")]
public class QuizController : ControllerBase
{
    private readonly IQuizService _quizService;
    private readonly ILogger<QuizController> _logger;

    public QuizController(
        IQuizService quizService,
        ILogger<QuizController> logger)
    {
        _quizService = quizService;
        _logger = logger;
    }

    /// <summary>
    /// აბრუნებს ავტორიზებული ლექტორის მიერ შექმნილ ყველა ტესტს.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var quizzes = await _quizService.GetByLecturerAsync(User.GetUserId());

        return Ok(quizzes);
    }

    /// <summary>
    /// აბრუნებს კონკრეტულ ტესტს ID-ის მიხედვით.
    /// წვდომა აქვს მხოლოდ ტესტის შემქმნელ ლექტორს.
    /// </summary>
    /// <param name="id">ტესტის ID</param>
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var quiz = await _quizService.GetOwnedAsync(id, User.GetUserId());

        return Ok(quiz);
    }

    /// <summary>
    /// ქმნის ახალ ტესტს.
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> Create(
        [FromBody] CreateQuizDTO request)
    {
        var created = await _quizService.CreateAsync(request, User.GetUserId());

        return CreatedAtAction(
            nameof(GetById),
            new { id = created.Id },
            created);
    }

    /// <summary>
    /// ანახლებს არსებული ტესტის მონაცემებს.
    /// ტესტის შეცვლა შეუძლია მხოლოდ მის შემქმნელ ლექტორს,
    /// თუ ტესტი გამოქვეყნებული არ არის და მასზე მცდელობები არ არსებობს.
    /// </summary>
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(
        int id,
        [FromBody] UpdateQuizDTO request)
    {
        var updated = await _quizService.UpdateAsync(id, request, User.GetUserId());

        return Ok(updated);
    }

    /// <summary>
    /// შლის ტესტს მხოლოდ მისი შემქმნელი ლექტორი,
    /// თუ ტესტი გამოქვეყნებული არ არის და არ აქვს მცდელობები.
    /// </summary>
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        await _quizService.DeleteAsync(id, User.GetUserId());

        return NoContent();
    }

    /// <summary>
    /// აქვეყნებს ტესტს სტუდენტებისთვის ხელმისაწვდომად.
    /// გამოქვეყნება შეუძლია მხოლოდ ტესტის შემქმნელ ლექტორს.
    /// </summary>
    [HttpPatch("{id:int}/publish")]
    public async Task<IActionResult> Publish(int id)
    {
        var lecturerId = User.GetUserId();

        await _quizService.PublishAsync(id, lecturerId);

        _logger.LogInformation(
            "Lecturer {LecturerId} published quiz {QuizId}",
            lecturerId, id);

        return Ok(new { message = "Quiz published successfully." });
    }

    /// <summary>
    /// ხურავს გამოქვეყნებულ ტესტს.
    /// დახურვა შეუძლია მხოლოდ ტესტის შემქმნელ ლექტორს.
    /// </summary>
    [HttpPatch("{id:int}/close")]
    public async Task<IActionResult> Close(int id)
    {
        var lecturerId = User.GetUserId();

        await _quizService.CloseAsync(id, lecturerId);

        _logger.LogInformation(
            "Lecturer {LecturerId} closed quiz {QuizId}",
            lecturerId, id);

        return Ok(new { message = "Quiz closed successfully." });
    }

    /// <summary>
    /// აბრუნებს კონკრეტული ლექტორის მიერ შექმნილ ყველა ტესტს.
    /// </summary>
    [HttpGet("my-quizzes")]
    public async Task<IActionResult> GetMyQuizzes()
    {
        var quizzes = await _quizService.GetByLecturerAsync(User.GetUserId());

        return Ok(quizzes);
    }
}
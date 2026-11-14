using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuizMaster.Domain.DTOs.Questions;
using QuizMaster.Domain.Entities;
using QuizMaster.Service.Interfaces;
using System.Security.Claims;

namespace QuizMaster.API.Controllers;

/// <summary>
/// მართავს პასუხების ვარიანტებს.
/// </summary>
[Route("api/[controller]")]
[ApiController]
[Authorize(Roles = "Lecturer")]
public class AnswerOptionController : ControllerBase
{
    private readonly IAnswerOptionService _answerOptionService;
    private readonly IQuestionService _questionService;
    private readonly IQuizService _quizService;

    public AnswerOptionController(
        IAnswerOptionService answerOptionService,
        IQuestionService questionService,
        IQuizService quizService)
    {
        _answerOptionService = answerOptionService;
        _questionService = questionService;
        _quizService = quizService;
    }

    [HttpGet("question/{questionId}")]
    public async Task<IActionResult> GetByQuestion(int questionId)
    {
        var question = await _questionService.GetByIdAsync(questionId);

        if (question == null)
            return Problem(statusCode: 404, title: "Question not found.");

        var quiz = await _quizService.GetByIdAsync(question.QuizId);

        if (quiz == null)
            return Problem(statusCode: 404, title: "Quiz not found.");

        if (!IsQuizOwnedByCurrentLecturer(quiz))
            return Forbid();

        var options = question.AnswerOptions.Select(a => new
        {
            a.Id,
            a.QuestionId,
            a.Text,
            a.IsCorrect
        });

        return Ok(options);
    }
    /// <summary>
    /// ამატებს პასუხის ახალ ვარიანტს.
    /// </summary>
    [HttpPost("question/{questionId}")]
    public async Task<IActionResult> Create(
        int questionId,
        [FromBody] AnswerOptionDTO dto)
    {
        var question = await _questionService.GetByIdAsync(questionId);

        if (question == null)
            return Problem(statusCode: 404, title: "Question not found.");

        var quiz = await _quizService.GetByIdAsync(question.QuizId);

        if (quiz == null)
            return Problem(statusCode: 404, title: "Quiz not found.");
        if (!IsQuizOwnedByCurrentLecturer(quiz))
            return Forbid();
        if (quiz.IsPublished)
            return Problem(statusCode: 400, title: "Cannot modify a published quiz.");

        var hasAttempts = await _quizService.HasAttemptsAsync(quiz.Id);

        if (hasAttempts)
            return Problem(statusCode: 400, title: "Cannot modify a quiz that students have already attempted.");

        var option = new AnswerOption
        {
            QuestionId = questionId,
            Text = dto.Text,
            IsCorrect = dto.IsCorrect
        };

        var created = await _answerOptionService.AddAsync(option);

        if (!created)
            return Problem(statusCode: 400, title: "Cannot add an answer option because students have already answered this question.");

        return Ok(new { message = "Answer option created successfully." });
    }
    /// <summary>
    /// ამოწმებს, ეკუთვნის თუ არა Quiz ავტორიზებულ ლექტორს.
    /// </summary>
    private bool IsQuizOwnedByCurrentLecturer(Quiz quiz)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (!int.TryParse(userId, out int lecturerId))
            return false;

        return quiz.LecturerId == lecturerId;
    }
    /// <summary>
    /// ანახლებს პასუხის ვარიანტს.
    /// </summary>
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(
        int id,
        [FromBody] AnswerOptionDTO dto)
    {
        var option = await _answerOptionService.GetByIdAsync(id);

        if (option == null)
            return Problem(statusCode: 404, title: "Answer option not found.");

        var question = await _questionService.GetByIdAsync(option.QuestionId);

        if (question == null)
            return Problem(statusCode: 404, title: "Question not found.");

        var quiz = await _quizService.GetByIdAsync(question.QuizId);

        if (quiz == null)
            return Problem(statusCode: 404, title: "Quiz not found.");
        if (!IsQuizOwnedByCurrentLecturer(quiz))
            return Forbid();
        if (quiz.IsPublished)
            return Problem(statusCode: 400, title: "Cannot modify a published quiz.");

        var hasAttempts = await _quizService.HasAttemptsAsync(quiz.Id);

        if (hasAttempts)
            return Problem(statusCode: 400, title: "Cannot modify a quiz that students have already attempted.");

        option.Text = dto.Text;
        option.IsCorrect = dto.IsCorrect;

        var updated = await _answerOptionService.UpdateAsync(option);

        if (!updated)
            return Problem(statusCode: 400, title: "Cannot update this answer option because students have already answered it.");

        return Ok(new { message = "Answer option updated successfully." });
    }

    /// <summary>
    /// შლის პასუხის ვარიანტს.
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var option = await _answerOptionService.GetByIdAsync(id);

        if (option == null)
            return Problem(statusCode: 404, title: "Answer option not found.");

        var question = await _questionService.GetByIdAsync(option.QuestionId);

        if (question == null)
            return Problem(statusCode: 404, title: "Question not found.");

        var quiz = await _quizService.GetByIdAsync(question.QuizId);

        if (quiz == null)
            return Problem(statusCode: 404, title: "Quiz not found.");
        if (!IsQuizOwnedByCurrentLecturer(quiz))
            return Forbid();
        if (quiz.IsPublished)
            return Problem(statusCode: 400, title: "Cannot modify a published quiz.");

        var hasAttempts = await _quizService.HasAttemptsAsync(quiz.Id);

        if (hasAttempts)
            return Problem(statusCode: 400, title: "Cannot modify a quiz that students have already attempted.");

        var deleted = await _answerOptionService.DeleteAsync(id);

        if (!deleted)
            return Problem(statusCode: 400, title: "Cannot delete this answer option because students have already answered it.");

        return Ok(new { message = "Answer option deleted successfully." });
    }
}
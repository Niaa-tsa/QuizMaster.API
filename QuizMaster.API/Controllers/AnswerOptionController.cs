using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuizMaster.API.Extensions;
using QuizMaster.Domain.DTOs.Questions;
using QuizMaster.Service.Interfaces;

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

    public AnswerOptionController(IAnswerOptionService answerOptionService)
    {
        _answerOptionService = answerOptionService;
    }

    /// <summary>
    /// აბრუნებს კითხვის პასუხის ვარიანტებს (სწორი პასუხების ჩათვლით).
    /// </summary>
    [HttpGet("question/{questionId}")]
    public async Task<IActionResult> GetByQuestion(int questionId)
    {
        var options = await _answerOptionService.GetByQuestionAsync(
            questionId, User.GetUserId());

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
        var created = await _answerOptionService.CreateAsync(
            questionId, dto, User.GetUserId());

        return CreatedAtAction(
            nameof(GetByQuestion),
            new { questionId },
            created);
    }

    /// <summary>
    /// ანახლებს პასუხის ვარიანტს.
    /// </summary>
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(
        int id,
        [FromBody] AnswerOptionDTO dto)
    {
        await _answerOptionService.UpdateAsync(id, dto, User.GetUserId());

        return Ok(new { message = "Answer option updated successfully." });
    }

    /// <summary>
    /// შლის პასუხის ვარიანტს.
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        await _answerOptionService.DeleteAsync(id, User.GetUserId());

        return Ok(new { message = "Answer option deleted successfully." });
    }
}
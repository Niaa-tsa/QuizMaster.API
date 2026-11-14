using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuizMaster.API.Extensions;
using QuizMaster.Domain.DTOs.Questions;
using QuizMaster.Service.Interfaces;

namespace QuizMaster.API.Controllers;

/// <summary>
/// მართავს ქვიზის კითხვებს.
/// </summary>
[Route("api/[controller]")]
[ApiController]
[Authorize(Roles = "Lecturer")]
public class QuestionController : ControllerBase
{
    private readonly IQuestionService _questionService;
    private readonly IValidator<CreateQuestionDTO> _createValidator;
    private readonly IValidator<UpdateQuestionDTO> _updateValidator;

    public QuestionController(
        IQuestionService questionService,
        IValidator<CreateQuestionDTO> createValidator,
        IValidator<UpdateQuestionDTO> updateValidator)
    {
        _questionService = questionService;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
    }

    /// <summary>
    /// აბრუნებს ავტორიზებული ლექტორის ყველა კითხვას.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var questions = await _questionService.GetByLecturerAsync(User.GetUserId());

        return Ok(questions);
    }

    /// <summary>
    /// აბრუნებს კონკრეტულ კითხვას.
    /// </summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var question = await _questionService.GetOwnedAsync(id, User.GetUserId());

        return Ok(question);
    }

    /// <summary>
    /// ამატებს ახალ კითხვას ქვიზში.
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> Create(
        [FromBody] CreateQuestionDTO dto,
        [FromQuery] int quizId)
    {
        var validation = await _createValidator.ValidateAsync(dto);

        if (!validation.IsValid)
            return ValidationProblem(
                new ValidationProblemDetails(validation.ToDictionary()));

        var created = await _questionService.CreateAsync(
            quizId, dto, User.GetUserId());

        return CreatedAtAction(
            nameof(GetById),
            new { id = created.Id },
            created);
    }

    /// <summary>
    /// ანახლებს არსებულ კითხვას.
    /// </summary>
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(
        int id,
        [FromBody] UpdateQuestionDTO dto)
    {
        var validation = await _updateValidator.ValidateAsync(dto);

        if (!validation.IsValid)
            return ValidationProblem(
                new ValidationProblemDetails(validation.ToDictionary()));

        await _questionService.UpdateAsync(id, dto, User.GetUserId());

        return Ok(new { message = "Question updated successfully." });
    }

    /// <summary>
    /// შლის არსებულ კითხვას.
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        await _questionService.DeleteAsync(id, User.GetUserId());

        return Ok(new { message = "Question deleted successfully." });
    }
}
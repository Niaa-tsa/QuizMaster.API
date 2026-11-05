
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuizMaster.Domain.DTOs.Questions;
using QuizMaster.Domain.Entities;
using QuizMaster.Service.Interfaces;

namespace QuizMaster.API.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize(Roles = "Lecturer")]
public class QuestionController : ControllerBase
{
    private readonly IQuestionService _questionService;
    private readonly IQuizService _quizService;
    private readonly IValidator<CreateQuestionDTO> _createValidator;
    private readonly IValidator<UpdateQuestionDTO> _updateValidator;

    public QuestionController(
        IQuestionService questionService,
        IQuizService quizService,
        IValidator<CreateQuestionDTO> createValidator,
        IValidator<UpdateQuestionDTO> updateValidator)
    {
        _questionService = questionService;
        _quizService = quizService;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
    }
    // ყველა კითხვის მიღება
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var questions = await _questionService.GetAllAsync();

        var result = questions.Select(q => new QuestionResponseDTO
        {
            Id = q.Id,
            Text = q.Text,
            Type = q.Type,
            Points = q.Points,
            AnswerOptions = q.AnswerOptions.Select(a =>
                new AnswerOptionResponseDTO
                {
                    Id = a.Id,
                    Text = a.Text
                }).ToList()
        }).ToList();

        return Ok(result);
    }

    // კონკრეტული კითხვის მიღება
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var question = await _questionService.GetByIdAsync(id);

        if (question == null)
            return NotFound();

        var result = new QuestionResponseDTO
        {
            Id = question.Id,
            Text = question.Text,
            Type = question.Type,
            Points = question.Points,
            AnswerOptions = question.AnswerOptions.Select(a =>
                new AnswerOptionResponseDTO
                {
                    Id = a.Id,
                    Text = a.Text
                }).ToList()
        };

        return Ok(result);
    }

    // კითხვის დამატება
    [HttpPost]
    public async Task<IActionResult> Create(
        [FromBody] CreateQuestionDTO dto,
        [FromQuery] int quizId)
    {
        var validation = await _createValidator.ValidateAsync(dto);

        if (!validation.IsValid)
            return ValidationProblem(
                new ValidationProblemDetails(validation.ToDictionary()));
        var quiz = await _quizService.GetByIdAsync(quizId);

        if (quiz == null)
            return NotFound("Quiz not found.");

        if (quiz.IsPublished)
            return BadRequest("Cannot modify a published quiz.");
        var hasAttempts = await _quizService.HasAttemptsAsync(quizId);

        if (hasAttempts)
            return BadRequest(
                "Cannot add questions because students have already attempted this quiz.");
        var question = new Question
        {
            QuizId = quizId,
            Text = dto.Text,
            Type = dto.Type,
            Points = dto.Points,
            AnswerOptions = dto.AnswerOptions.Select(a =>
                new AnswerOption
                {
                    Text = a.Text,
                    IsCorrect = a.IsCorrect
                }).ToList()
        };

     

        await _questionService.AddAsync(question);

        return Ok(new { message = "Question created successfully." });
    }

    // კითხვის განახლება
    // კითხვის განახლება
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(
        int id,
        [FromBody] UpdateQuestionDTO dto)
    {
        var validation = await _updateValidator.ValidateAsync(dto);

        if (!validation.IsValid)
            return ValidationProblem(
                new ValidationProblemDetails(validation.ToDictionary()));

        var question = await _questionService.GetByIdAsync(id);

        if (question == null)
            return NotFound();

        var quiz = await _quizService.GetByIdAsync(question.QuizId);

        if (quiz == null)
            return NotFound("Quiz not found.");

        if (quiz.IsPublished)
            return BadRequest("Cannot modify a published quiz.");

        var hasAttempts = await _quizService.HasAttemptsAsync(quiz.Id);

        if (hasAttempts)
            return BadRequest(
                "Cannot modify a quiz that students have already attempted.");

        var updatedQuestion = new Question
        {
            Id = id,
            QuizId = question.QuizId,
            Text = dto.Text,
            Type = dto.Type,
            Points = dto.Points,
            AnswerOptions = dto.AnswerOptions.Select(a =>
                new AnswerOption
                {
                    Id = a.Id ?? 0,
                    QuestionId = id,
                    Text = a.Text,
                    IsCorrect = a.IsCorrect
                }).ToList()
        };

        var updated = await _questionService.UpdateAsync(updatedQuestion);
        if (!updated)
            return BadRequest(
                "Question cannot be updated. Check answer option IDs or existing student answers.");
        return Ok(new { message = "Question updated successfully." });
    }

    // კითხვის წაშლა
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var question = await _questionService.GetByIdAsync(id);

        if (question == null)
            return NotFound();

        var quiz = await _quizService.GetByIdAsync(question.QuizId);

        if (quiz == null)
            return NotFound("Quiz not found.");

        if (quiz.IsPublished)
            return BadRequest("Cannot delete a question from a published quiz.");
        var hasAttempts = await _quizService.HasAttemptsAsync(quiz.Id);

        if (hasAttempts)
            return BadRequest(
                "Cannot modify a quiz that students have already attempted.");
        var deleted = await _questionService.DeleteAsync(id);
        if (!deleted)
            return BadRequest(
                "Question cannot be deleted because students have already answered it.");

        return Ok(new { message = "Question deleted successfully." });
 
    
    }
}


using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using QuizMaster.Domain.Entities;
using QuizMaster.Domain.DTOs.Questions;
using QuizMaster.Domain.Enums;
using QuizMaster.Service.Interfaces;

namespace QuizMaster.API.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize(Roles = "Lecturer")]
public class QuestionController : ControllerBase
{
    private readonly IQuestionService _questionService;
    private readonly IQuizService _quizService;

    public QuestionController(
        IQuestionService questionService,
        IQuizService quizService)
    {
        _questionService = questionService;
        _quizService = quizService;
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

        if (!IsValidQuestion(question))
            return BadRequest("Invalid answer options for this question type.");

        await _questionService.AddAsync(question);

        return Ok(new { message = "Question created successfully." });
    }

    // კითხვის განახლება
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(
        int id,
        [FromBody] UpdateQuestionDTO dto)
    {
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
        if (!IsValidQuestionOptions(dto.Type, dto.AnswerOptions))
            return BadRequest("Invalid answer options for this question type.");

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

    private bool IsValidQuestion(Question question)
    {
        return IsValidQuestionOptions(
            question.Type,
            question.AnswerOptions.Select(a =>
                new AnswerOptionDTO
                {
                    Text = a.Text,
                    IsCorrect = a.IsCorrect
                }).ToList());
    }

    private bool IsValidQuestionOptions(
        QuestionType type,
        List<AnswerOptionDTO> options)
    {
        if (options.Count < 2)
            return false;

        int correctCount = options.Count(o => o.IsCorrect);

        return type switch
        {
            QuestionType.SingleChoice => correctCount == 1,
            QuestionType.MultipleChoice => correctCount >= 1,
            QuestionType.TrueFalse => options.Count == 2 && correctCount == 1,
            _ => false
        };
    }
}

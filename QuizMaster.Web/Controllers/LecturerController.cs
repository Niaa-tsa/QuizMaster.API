using Microsoft.AspNetCore.Mvc;
using QuizMaster.Domain.Entities;
using QuizMaster.Service.Interfaces;
using QuizMaster.Web.Models;

namespace QuizMaster.Web.Controllers
{
    public class LecturerController : Controller
    {
        private readonly IQuizService _quizService;
        private readonly IQuestionService _questionService;

        public LecturerController(
            IQuizService quizService,
            IQuestionService questionService)
        {
            _quizService = quizService;
            _questionService = questionService;
        }

        public IActionResult Index()
        {
            return View();
        }

        public async Task<IActionResult> MyQuizzes()
        {
            var quizzes = await _quizService.GetAllAsync();
            return View(quizzes);
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateQuizViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var quiz = new Quiz
            {
                Title = model.Title,
                TimeLimitMinutes = model.TimeLimitMinutes,
                IsPublished = false,
                CreatedAt = DateTime.UtcNow
            };

            await _quizService.AddAsync(quiz);

            return RedirectToAction(nameof(MyQuizzes));
        }

        // კონკრეტული ქვიზის კითხვების ნახვა
        public async Task<IActionResult> Questions(int quizId)
        {
            var quiz = await _quizService.GetByIdAsync(quizId);

            if (quiz == null)
                return NotFound();

            var questions = await _questionService.GetByQuizIdAsync(quizId);

            ViewBag.QuizTitle = quiz.Title;
            ViewBag.QuizId = quizId;

            return View(questions);
        }

        // კითხვის დამატების გვერდის გახსნა
        [HttpGet]
        public async Task<IActionResult> AddQuestion(int quizId)
        {
            var quiz = await _quizService.GetByIdAsync(quizId);

            if (quiz == null)
                return NotFound();

            var model = new CreateQuestionViewModel
            {
                QuizId = quizId,
                AnswerOptions = new List<CreateAnswerOptionViewModel>
                {
                    new CreateAnswerOptionViewModel(),
                    new CreateAnswerOptionViewModel(),
                    new CreateAnswerOptionViewModel(),
                    new CreateAnswerOptionViewModel()
                }
            };

            return View(model);
        }

        // კითხვის შენახვა
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddQuestion(
            CreateQuestionViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var quiz = await _quizService.GetByIdAsync(model.QuizId);

            if (quiz == null)
                return NotFound();

            var question = new Question
            {
                QuizId = model.QuizId,
                Text = model.Text,
                Type = model.Type,
                Points = model.Points,

                AnswerOptions = model.AnswerOptions
                    .Select(option => new AnswerOption
                    {
                        Text = option.Text,
                        IsCorrect = option.IsCorrect
                    })
                    .ToList()
            };

            await _questionService.AddAsync(question);

            return RedirectToAction(
                nameof(Questions),
                new { quizId = model.QuizId });
        }

        // კითხვის რედაქტირების გვერდის გახსნა
        [HttpGet]
        public async Task<IActionResult> EditQuestion(int id)
        {
            var question = await _questionService.GetByIdAsync(id);

            if (question == null)
                return NotFound();

            var model = new EditQuestionViewModel
            {
                Id = question.Id,
                QuizId = question.QuizId,
                Text = question.Text,
                Type = question.Type,
                Points = question.Points,

                AnswerOptions = question.AnswerOptions
                    .Select(option => new EditAnswerOptionViewModel
                    {
                        Id = option.Id,
                        Text = option.Text,
                        IsCorrect = option.IsCorrect
                    })
                    .ToList()
            };

            return View(model);
        }

        // კითხვის ცვლილებების შენახვა
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditQuestion(
            EditQuestionViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var question = new Question
            {
                Id = model.Id,
                QuizId = model.QuizId,
                Text = model.Text,
                Type = model.Type,
                Points = model.Points,

                AnswerOptions = model.AnswerOptions
                    .Select(option => new AnswerOption
                    {
                        Id = option.Id,
                        Text = option.Text,
                        IsCorrect = option.IsCorrect
                    })
                    .ToList()
            };

            var updated = await _questionService.UpdateAsync(question);

            if (!updated)
            {
                ModelState.AddModelError(
                    string.Empty,
                    "Question could not be updated. It may already have student answers."
                );

                return View(model);
            }

            return RedirectToAction(
                nameof(Questions),
                new { quizId = model.QuizId });
        }

        // კითხვის წაშლა
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteQuestion(int id, int quizId)
        {
            var deleted = await _questionService.DeleteAsync(id);

            if (!deleted)
            {
                TempData["Error"] =
                    "Question could not be deleted. It may already have student answers.";
            }
            else
            {
                TempData["Success"] = "Question deleted successfully.";
            }

            return RedirectToAction(
                nameof(Questions),
                new { quizId = quizId });
        }
    }
}
using Microsoft.AspNetCore.Mvc;

namespace QuizMaster.Web.Controllers
{
    public class StudentController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Quizzes()
        {
            return View();
        }

        public IActionResult History()
        {
            return View();
        }
    }
}
using Microsoft.EntityFrameworkCore;
using QuizMaster.Domain.Entities;
using QuizMaster.Infrastructure.Data;
using QuizMaster.Service.Interfaces.Repositories;

namespace QuizMaster.Infrastructure.Repositories
{
    public class QuizRepository : IQuizRepository
    {
        private readonly QuizMasterDbContext _context;

        public QuizRepository(QuizMasterDbContext context)
        {
            _context = context;
        }

        public async Task AddQuizAsync(Quiz quiz)
        {
            await _context.Quizzes.AddAsync(quiz);
        }

        public async Task<ICollection<Quiz>> GetAllQuizzesAsync()
        {
            return await _context.Quizzes
                .Include(q => q.Questions)
                .ToListAsync();
        }

        public async Task<ICollection<Quiz>> GetPublishedQuizzesAsync()
        {
            return await _context.Quizzes
                .Where(q => q.IsPublished)
                .ToListAsync();
        }

        public async Task<Quiz?> GetQuizByIdAsync(int id)
        {
            return await _context.Quizzes
                .Include(q => q.Questions)
                .FirstOrDefaultAsync(q => q.Id == id);
        }

        public async Task UpdateQuizAsync(int id, Quiz quiz)
        {
            var existingQuiz = await _context.Quizzes
                .FirstOrDefaultAsync(q => q.Id == id);

            if (existingQuiz == null)
                throw new ArgumentException("Quiz not found");

            existingQuiz.Title = quiz.Title;
            existingQuiz.TimeLimitMinutes = quiz.TimeLimitMinutes;
            existingQuiz.IsPublished = quiz.IsPublished;
        }
        public async Task DeleteQuizAsync(int id)
        {
            var quiz = await _context.Quizzes
                .FirstOrDefaultAsync(q => q.Id == id);

            if (quiz == null)
                throw new ArgumentException("Quiz not found");

            _context.Quizzes.Remove(quiz);
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
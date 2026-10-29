using Microsoft.EntityFrameworkCore;
using QuizMaster.Domain.Entities;
using QuizMaster.Infrastructure.Data;
using QuizMaster.Service.Interfaces.Repositories;

namespace QuizMaster.Infrastructure.Repositories
{
    public class QuizAttemptRepository : IQuizAttemptRepository
    {
        private readonly QuizMasterDbContext _context;

        public QuizAttemptRepository(QuizMasterDbContext context)
        {
            _context = context;
        }

        public async Task AddQuizAttemptAsync(QuizAttempt quizAttempt)
        {
            await _context.QuizAttempts.AddAsync(quizAttempt);
        }

        public async Task<ICollection<QuizAttempt>> GetAllQuizAttemptsAsync()
        {
            return await _context.QuizAttempts
                .Include(a => a.User)
                .Include(a => a.Quiz)
                .ToListAsync();
        }

        public async Task<QuizAttempt?> GetQuizAttemptByIdAsync(int id)
        {
            return await _context.QuizAttempts
                .Include(a => a.User)
                .Include(a => a.Quiz)
                .Include(a => a.StudentAnswers)
                .Include(a => a.QuizResult)
                .FirstOrDefaultAsync(a => a.Id == id);
        }

        public async Task UpdateQuizAttemptAsync(
        int id, QuizAttempt quizAttempt)
        {
            var existingAttempt = await _context.QuizAttempts
                .FirstOrDefaultAsync(a => a.Id == id);

            if (existingAttempt == null)
                throw new ArgumentException("Quiz attempt not found");

            existingAttempt.UserId = quizAttempt.UserId;
            existingAttempt.QuizId = quizAttempt.QuizId;
            existingAttempt.StartedAt = quizAttempt.StartedAt;
            existingAttempt.FinishedAt = quizAttempt.FinishedAt;
            existingAttempt.Status = quizAttempt.Status;
        }
        public async Task DeleteQuizAttemptAsync(int id)
        {
            var quizAttempt = await _context.QuizAttempts
                .FirstOrDefaultAsync(a => a.Id == id);

            if (quizAttempt == null)
                throw new ArgumentException("Quiz attempt not found");

            _context.QuizAttempts.Remove(quizAttempt);
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
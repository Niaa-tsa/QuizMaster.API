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

        public async Task UpdateQuizAttemptAsync(
            int id,
            QuizAttempt quizAttempt)
        {
            var existingAttempt = await _context.QuizAttempts
                .FirstOrDefaultAsync(a => a.Id == id);

            if (existingAttempt == null)
                throw new ArgumentException("Quiz attempt not found");

            existingAttempt.FinishedAt = quizAttempt.FinishedAt;
            existingAttempt.Status = quizAttempt.Status;
        }

        public async Task<bool> HasAttemptsForQuizAsync(int quizId)
        {
            return await _context.QuizAttempts
                .AnyAsync(a => a.QuizId == quizId);
        }

        public async Task<QuizAttempt?> GetStudentAttemptAsync(
            int attemptId,
            int userId)
        {
            return await _context.QuizAttempts
                .Include(a => a.Quiz)
                    .ThenInclude(q => q.Questions)
                        .ThenInclude(q => q.AnswerOptions)
                .Include(a => a.StudentAnswers)
                .Include(a => a.QuizResult)
                .FirstOrDefaultAsync(a =>
                    a.Id == attemptId &&
                    a.UserId == userId);
        }

        public async Task<ICollection<QuizAttempt>> GetStudentAttemptsAsync(
            int userId)
        {
            return await _context.QuizAttempts
                .Include(a => a.Quiz)
                .Include(a => a.QuizResult)
                .Where(a => a.UserId == userId)
                .OrderByDescending(a => a.StartedAt)
                .ToListAsync();
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
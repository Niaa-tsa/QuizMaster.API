using Microsoft.EntityFrameworkCore;
using QuizMaster.Domain.Entities;
using QuizMaster.Infrastructure.Data;
using QuizMaster.Service.Interfaces.Repositories;

namespace QuizMaster.Infrastructure.Repositories
{
    public class QuizResultRepository : IQuizResultRepository
    {
        private readonly QuizMasterDbContext _context;

        public QuizResultRepository(QuizMasterDbContext context)
        {
            _context = context;
        }

        public async Task AddQuizResultAsync(QuizResult quizResult)
        {
            await _context.QuizResults.AddAsync(quizResult);
        }

        public async Task<ICollection<QuizResult>> GetAllQuizResultsAsync()
        {
            return await _context.QuizResults
                .Include(r => r.QuizAttempt)
                .ToListAsync();
        }

        public async Task<QuizResult?> GetQuizResultByIdAsync(int id)
        {
            return await _context.QuizResults
                .Include(r => r.QuizAttempt)
                .FirstOrDefaultAsync(r => r.Id == id);
        }

        public async Task UpdateQuizResultAsync(int id, QuizResult quizResult)
        {
            var existingResult = await _context.QuizResults
                .FirstOrDefaultAsync(r => r.Id == id);

            if (existingResult == null)
                throw new ArgumentException("Quiz result not found");
            existingResult.AttemptId = quizResult.AttemptId;
            existingResult.Score = quizResult.Score;
            existingResult.MaxScore = quizResult.MaxScore;
        }

        public async Task DeleteQuizResultAsync(int id)
        {
            var quizResult = await _context.QuizResults
                .FirstOrDefaultAsync(r => r.Id == id);

            if (quizResult == null)
                throw new ArgumentException("Quiz result not found");

            _context.QuizResults.Remove(quizResult);
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
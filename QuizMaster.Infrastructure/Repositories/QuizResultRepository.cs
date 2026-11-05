using Microsoft.EntityFrameworkCore;
using QuizMaster.Domain.DTOs.Quizzes;
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
        public async Task<ICollection<QuizResult>> GetLeaderboardAsync(
    int quizId,
    int page,
    int pageSize)
        {
            return await _context.QuizResults
                .Include(r => r.QuizAttempt)
                    .ThenInclude(a => a.User)
                .Where(r => r.QuizAttempt.QuizId == quizId &&
                            r.QuizAttempt.Status == "Finished")
                .OrderByDescending(r => r.Score)
                .ThenBy(r => r.QuizAttempt.FinishedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }
        public async Task<QuizStatisticsDTO?> GetQuizStatisticsAsync(int quizId)
        {
            return await _context.QuizResults
                .Where(r => r.QuizAttempt.QuizId == quizId &&
                            r.QuizAttempt.Status == "Finished")
                .GroupBy(r => r.QuizAttempt.QuizId)
                .Select(g => new QuizStatisticsDTO
                {
                    QuizId = g.Key,
                    AttemptsCount = g.Count(),
                    MaxScore = g.Max(r => r.MaxScore),
                    AverageScore = g.Average(r => r.Score),
                    HighestScore = g.Max(r => r.Score),
                    LowestScore = g.Min(r => r.Score),
                    TotalScore = g.Sum(r => r.Score)
                })
                .FirstOrDefaultAsync();
        }
        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
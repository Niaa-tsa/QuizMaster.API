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

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
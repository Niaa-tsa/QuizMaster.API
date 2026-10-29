using Microsoft.EntityFrameworkCore;
using QuizMaster.Domain.Entities;
using QuizMaster.Infrastructure.Data;
using QuizMaster.Service.Interfaces.Repositories;

namespace QuizMaster.Infrastructure.Repositories
{
    public class AnswerOptionRepository : IAnswerOptionRepository
    {
        private readonly QuizMasterDbContext _context;

        public AnswerOptionRepository(QuizMasterDbContext context)
        {
            _context = context;
        }

        public async Task AddAnswerOptionAsync(AnswerOption answerOption)
        {
            await _context.AnswerOptions.AddAsync(answerOption);
        }

        public async Task<ICollection<AnswerOption>> GetAllAnswerOptionsAsync()
        {
            return await _context.AnswerOptions
                .Include(a => a.Question)
                .ToListAsync();
        }

        public async Task<AnswerOption?> GetAnswerOptionByIdAsync(int id)
        {
            return await _context.AnswerOptions
                .Include(a => a.Question)
                .FirstOrDefaultAsync(a => a.Id == id);
        }

        public async Task UpdateAnswerOptionAsync(
            int id, AnswerOption answerOption)
        {
            var existingOption = await _context.AnswerOptions
                .FirstOrDefaultAsync(a => a.Id == id);

            if (existingOption == null)
                throw new ArgumentException("Answer option not found");

            existingOption.QuestionId = answerOption.QuestionId;
            existingOption.Text = answerOption.Text;
            existingOption.IsCorrect = answerOption.IsCorrect;
        }

        public async Task DeleteAnswerOptionAsync(int id)
        {
            var answerOption = await _context.AnswerOptions
                .FirstOrDefaultAsync(a => a.Id == id);

            if (answerOption == null)
                throw new ArgumentException("Answer option not found");

            _context.AnswerOptions.Remove(answerOption);
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
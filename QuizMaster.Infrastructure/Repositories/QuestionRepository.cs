using Microsoft.EntityFrameworkCore;
using QuizMaster.Domain.Entities;
using QuizMaster.Infrastructure.Data;
using QuizMaster.Service.Interfaces.Repositories;

namespace QuizMaster.Infrastructure.Repositories
{
    public class QuestionRepository : IQuestionRepository
    {
        private readonly QuizMasterDbContext _context;

        public QuestionRepository(QuizMasterDbContext context)
        {
            _context = context;
        }

        public async Task AddQuestionAsync(Question question)
        {
            await _context.Questions.AddAsync(question);
        }

        public async Task<ICollection<Question>> GetAllQuestionsAsync()
        {
            return await _context.Questions
                .Include(q => q.AnswerOptions)
                .ToListAsync();
        }

        public async Task<Question?> GetQuestionByIdAsync(int id)
        {
            return await _context.Questions
                .Include(q => q.AnswerOptions)
                .FirstOrDefaultAsync(q => q.Id == id);
        }

        public async Task UpdateQuestionAsync(int id, Question question)
        {
            var existingQuestion = await _context.Questions
                .FirstOrDefaultAsync(q => q.Id == id);

            if (existingQuestion == null)
                throw new ArgumentException("Question not found");

            existingQuestion.QuizId = question.QuizId;
            existingQuestion.Text = question.Text;
            existingQuestion.Type = question.Type;
            existingQuestion.Points = question.Points;
        }

        public async Task DeleteQuestionAsync(int id)
        {
            var question = await _context.Questions
                .FirstOrDefaultAsync(q => q.Id == id);

            if (question == null)
                throw new ArgumentException("Question not found");

            _context.Questions.Remove(question);
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
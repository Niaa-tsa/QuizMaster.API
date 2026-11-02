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
                .Include(q => q.AnswerOptions)
                .FirstOrDefaultAsync(q => q.Id == id);

            if (existingQuestion == null)
                return;

            existingQuestion.Text = question.Text;
            existingQuestion.Type = question.Type;
            existingQuestion.Points = question.Points;

            // ვადგენთ, რომელი ძველი პასუხები აღარ არის საჭირო
            var updatedOptionIds = question.AnswerOptions
                .Where(a => a.Id != 0)
                .Select(a => a.Id)
                .ToHashSet();

            var optionsToRemove = existingQuestion.AnswerOptions
                .Where(a => !updatedOptionIds.Contains(a.Id))
                .ToList();

            foreach (var option in optionsToRemove)
            {
                _context.AnswerOptions.Remove(option);
            }

            // ვაახლებთ არსებულ პასუხებს ან ვამატებთ ახალს
            foreach (var option in question.AnswerOptions)
            {
                var existingOption = existingQuestion.AnswerOptions
                    .FirstOrDefault(a => a.Id == option.Id && option.Id != 0);

                if (existingOption != null)
                {
                    existingOption.Text = option.Text;
                    existingOption.IsCorrect = option.IsCorrect;
                }
                else
                {
                    existingQuestion.AnswerOptions.Add(new AnswerOption
                    {
                        Text = option.Text,
                        IsCorrect = option.IsCorrect
                    });
                }
            }

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
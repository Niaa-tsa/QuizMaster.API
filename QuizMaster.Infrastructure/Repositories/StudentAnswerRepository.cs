using Microsoft.EntityFrameworkCore;
using QuizMaster.Domain.Entities;
using QuizMaster.Infrastructure.Data;
using QuizMaster.Service.Interfaces.Repositories;

namespace QuizMaster.Infrastructure.Repositories
{
    public class StudentAnswerRepository : IStudentAnswerRepository
    {
        private readonly QuizMasterDbContext _context;

        public StudentAnswerRepository(QuizMasterDbContext context)
        {
            _context = context;
        }

        public async Task AddStudentAnswerAsync(StudentAnswer studentAnswer)
        {
            await _context.StudentAnswers.AddAsync(studentAnswer);
        }

        public async Task<ICollection<StudentAnswer>> GetAllStudentAnswersAsync()
        {
            return await _context.StudentAnswers
                .Include(a => a.Question)
                .Include(a => a.AnswerOption)
                .ToListAsync();
        }

        public async Task<StudentAnswer?> GetStudentAnswerByIdAsync(int id)
        {
            return await _context.StudentAnswers
                .Include(a => a.Question)
                .Include(a => a.AnswerOption)
                .FirstOrDefaultAsync(a => a.Id == id);
        }

        public async Task UpdateStudentAnswerAsync(
    int id, StudentAnswer studentAnswer)
        {
            var existingAnswer = await _context.StudentAnswers
                .FirstOrDefaultAsync(a => a.Id == id);

            if (existingAnswer == null)
            {
                throw new ArgumentException("Student answer not found");
            }

            existingAnswer.AttemptId = studentAnswer.AttemptId;
            existingAnswer.QuestionId = studentAnswer.QuestionId;
            existingAnswer.AnswerOptionId = studentAnswer.AnswerOptionId;
        }

        public async Task DeleteStudentAnswerAsync(int id)
        {
            var studentAnswer = await _context.StudentAnswers
                .FirstOrDefaultAsync(a => a.Id == id);

            if (studentAnswer == null)
                throw new ArgumentException("Student answer not found");

            _context.StudentAnswers.Remove(studentAnswer);
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
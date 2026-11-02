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

        public async Task<ICollection<StudentAnswer>> GetAnswersByAttemptAndQuestionAsync(
            int attemptId,
            int questionId)
        {
            return await _context.StudentAnswers
                .Where(a => a.AttemptId == attemptId &&
                            a.QuestionId == questionId)
                .ToListAsync();
        }

        public async Task<bool> HasAnswersForQuestionAsync(int questionId)
        {
            return await _context.StudentAnswers
                .AnyAsync(a => a.QuestionId == questionId);
        }

        public async Task<bool> HasAnswersForOptionAsync(int answerOptionId)
        {
            return await _context.StudentAnswers
                .AnyAsync(a => a.AnswerOptionId == answerOptionId);
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
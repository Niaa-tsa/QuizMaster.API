using Microsoft.EntityFrameworkCore;
using QuizMaster.Domain.Entities;
using QuizMaster.Infrastructure.Data;
using QuizMaster.Service.Interfaces.Repositories;

namespace QuizMaster.Infrastructure.Repositories;

public class QuizAttemptRepository : IQuizAttemptRepository
{
    private readonly QuizMasterDbContext _context;

    public QuizAttemptRepository(QuizMasterDbContext context)
    {
        _context = context;
    }

    public async Task<List<QuizAttempt>> GetAllAsync() =>
        await _context.QuizAttempts.ToListAsync();

    public async Task<QuizAttempt?> GetByIdAsync(int id) =>
        await _context.QuizAttempts.FindAsync(id);

    public async Task AddAsync(QuizAttempt quizAttempt) =>
        await _context.QuizAttempts.AddAsync(quizAttempt);

    public void Update(QuizAttempt quizAttempt) =>
        _context.QuizAttempts.Update(quizAttempt);

    public void Delete(QuizAttempt quizAttempt) =>
        _context.QuizAttempts.Remove(quizAttempt);

    public async Task SaveChangesAsync() =>
        await _context.SaveChangesAsync();
}
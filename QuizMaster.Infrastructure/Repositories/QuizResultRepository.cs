using Microsoft.EntityFrameworkCore;
using QuizMaster.Domain.Entities;
using QuizMaster.Infrastructure.Data;
using QuizMaster.Service.Interfaces.Repositories;

namespace QuizMaster.Infrastructure.Repositories;

public class QuizResultRepository : IQuizResultRepository
{
    private readonly QuizMasterDbContext _context;

    public QuizResultRepository(QuizMasterDbContext context)
    {
        _context = context;
    }

    public async Task<List<QuizResult>> GetAllAsync() =>
        await _context.QuizResults.ToListAsync();

    public async Task<QuizResult?> GetByIdAsync(int id) =>
        await _context.QuizResults.FindAsync(id);

    public async Task AddAsync(QuizResult quizResult) =>
        await _context.QuizResults.AddAsync(quizResult);

    public void Update(QuizResult quizResult) =>
        _context.QuizResults.Update(quizResult);

    public void Delete(QuizResult quizResult) =>
        _context.QuizResults.Remove(quizResult);

    public async Task SaveChangesAsync() =>
        await _context.SaveChangesAsync();
}
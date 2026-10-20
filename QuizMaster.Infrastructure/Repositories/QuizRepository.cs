using Microsoft.EntityFrameworkCore;
using QuizMaster.Domain.Entities;
using QuizMaster.Infrastructure.Data;
using QuizMaster.Service.Interfaces.Repositories;

namespace QuizMaster.Infrastructure.Repositories;

public class QuizRepository : IQuizRepository
{
    private readonly QuizMasterDbContext _context;

    public QuizRepository(QuizMasterDbContext context)
    {
        _context = context;
    }

    public async Task<List<Quiz>> GetAllAsync() =>
        await _context.Quizzes.ToListAsync();

    public async Task<Quiz?> GetByIdAsync(int id) =>
        await _context.Quizzes.FindAsync(id);

    public async Task AddAsync(Quiz quiz) =>
        await _context.Quizzes.AddAsync(quiz);

    public void Update(Quiz quiz) => _context.Quizzes.Update(quiz);

    public void Delete(Quiz quiz) => _context.Quizzes.Remove(quiz);

    public async Task SaveChangesAsync() =>
        await _context.SaveChangesAsync();
}
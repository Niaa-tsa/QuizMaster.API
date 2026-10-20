using Microsoft.EntityFrameworkCore;
using QuizMaster.Domain.Entities;
using QuizMaster.Infrastructure.Data;
using QuizMaster.Service.Interfaces.Repositories;

namespace QuizMaster.Infrastructure.Repositories;

public class QuestionRepository : IQuestionRepository
{
    private readonly QuizMasterDbContext _context;

    public QuestionRepository(QuizMasterDbContext context)
    {
        _context = context;
    }

    public async Task<List<Question>> GetAllAsync() =>
        await _context.Questions.ToListAsync();

    public async Task<Question?> GetByIdAsync(int id) =>
        await _context.Questions.FindAsync(id);

    public async Task AddAsync(Question question) =>
        await _context.Questions.AddAsync(question);

    public void Update(Question question) =>
        _context.Questions.Update(question);

    public void Delete(Question question) =>
        _context.Questions.Remove(question);

    public async Task SaveChangesAsync() =>
        await _context.SaveChangesAsync();
}
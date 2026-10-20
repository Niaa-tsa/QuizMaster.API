using Microsoft.EntityFrameworkCore;
using QuizMaster.Domain.Entities;
using QuizMaster.Infrastructure.Data;
using QuizMaster.Service.Interfaces.Repositories;

namespace QuizMaster.Infrastructure.Repositories;

public class AnswerOptionRepository : IAnswerOptionRepository
{
    private readonly QuizMasterDbContext _context;

    public AnswerOptionRepository(QuizMasterDbContext context)
    {
        _context = context;
    }

    public async Task<List<AnswerOption>> GetAllAsync() =>
        await _context.AnswerOptions.ToListAsync();

    public async Task<AnswerOption?> GetByIdAsync(int id) =>
        await _context.AnswerOptions.FindAsync(id);

    public async Task AddAsync(AnswerOption answerOption) =>
        await _context.AnswerOptions.AddAsync(answerOption);

    public void Update(AnswerOption answerOption) =>
        _context.AnswerOptions.Update(answerOption);

    public void Delete(AnswerOption answerOption) =>
        _context.AnswerOptions.Remove(answerOption);

    public async Task SaveChangesAsync() =>
        await _context.SaveChangesAsync();
}
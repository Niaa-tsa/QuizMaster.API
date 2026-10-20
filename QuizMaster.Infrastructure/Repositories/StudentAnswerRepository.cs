using Microsoft.EntityFrameworkCore;
using QuizMaster.Domain.Entities;
using QuizMaster.Infrastructure.Data;
using QuizMaster.Service.Interfaces.Repositories;

namespace QuizMaster.Infrastructure.Repositories;

public class StudentAnswerRepository : IStudentAnswerRepository
{
    private readonly QuizMasterDbContext _context;

    public StudentAnswerRepository(QuizMasterDbContext context)
    {
        _context = context;
    }

    public async Task<List<StudentAnswer>> GetAllAsync() =>
        await _context.StudentAnswers.ToListAsync();

    public async Task<StudentAnswer?> GetByIdAsync(int id) =>
        await _context.StudentAnswers.FindAsync(id);

    public async Task AddAsync(StudentAnswer studentAnswer) =>
        await _context.StudentAnswers.AddAsync(studentAnswer);

    public void Update(StudentAnswer studentAnswer) =>
        _context.StudentAnswers.Update(studentAnswer);

    public void Delete(StudentAnswer studentAnswer) =>
        _context.StudentAnswers.Remove(studentAnswer);

    public async Task SaveChangesAsync() =>
        await _context.SaveChangesAsync();
}
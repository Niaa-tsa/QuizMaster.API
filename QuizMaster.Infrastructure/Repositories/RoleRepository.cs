using Microsoft.EntityFrameworkCore;
using QuizMaster.Domain.Entities;
using QuizMaster.Infrastructure.Data;
using QuizMaster.Service.Interfaces.Repositories;

namespace QuizMaster.Infrastructure.Repositories;

public class RoleRepository : IRoleRepository
{
    private readonly QuizMasterDbContext _context;

    public RoleRepository(QuizMasterDbContext context)
    {
        _context = context;
    }

    public async Task<List<Role>> GetAllAsync() =>
        await _context.Roles.ToListAsync();

    public async Task<Role?> GetByIdAsync(int id) =>
        await _context.Roles.FindAsync(id);

    public async Task AddAsync(Role role) =>
        await _context.Roles.AddAsync(role);

    public void Update(Role role) => _context.Roles.Update(role);

    public void Delete(Role role) => _context.Roles.Remove(role);

    public async Task SaveChangesAsync() =>
        await _context.SaveChangesAsync();
}
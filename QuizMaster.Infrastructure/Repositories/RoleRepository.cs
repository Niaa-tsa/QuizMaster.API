using Microsoft.EntityFrameworkCore;
using QuizMaster.Domain.Entities;
using QuizMaster.Infrastructure.Data;
using QuizMaster.Service.Interfaces.Repositories;

namespace QuizMaster.Infrastructure.Repositories
{
    public class RoleRepository : IRoleRepository
    {
        private readonly QuizMasterDbContext _context;

        public RoleRepository(QuizMasterDbContext context)
        {
            _context = context;
        }

        public async Task AddRoleAsync(Role role)
        {
            await _context.Roles.AddAsync(role);
        }

        public async Task<ICollection<Role>> GetAllRolesAsync()
        {
            return await _context.Roles
                .Include(r => r.Users)
                .ToListAsync();
        }

        public async Task<Role?> GetRoleByIdAsync(int id)
        {
            return await _context.Roles
                .Include(r => r.Users)
                .FirstOrDefaultAsync(r => r.Id == id);
        }

        public async Task UpdateRoleAsync(int id, Role role)
        {
            var existingRole = await _context.Roles
                .FirstOrDefaultAsync(r => r.Id == id);

            if (existingRole == null)
                throw new ArgumentException("Role not found");

            existingRole.Name = role.Name;
        }

        public async Task DeleteRoleAsync(int id)
        {
            var role = await _context.Roles
                .FirstOrDefaultAsync(r => r.Id == id);

            if (role == null)
                throw new ArgumentException("Role not found");

            _context.Roles.Remove(role);
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
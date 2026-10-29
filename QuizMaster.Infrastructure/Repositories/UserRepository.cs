using Microsoft.EntityFrameworkCore;
using QuizMaster.Domain.Entities;
using QuizMaster.Infrastructure.Data;
using QuizMaster.Service.Interfaces.Repositories;

namespace QuizMaster.Infrastructure.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly QuizMasterDbContext _context;

        public UserRepository(QuizMasterDbContext context)
        {
            _context = context;
        }

        public async Task AddUserAsync(User user)
        {
            await _context.Users.AddAsync(user);
        }

        public async Task<ICollection<User>> GetAllUsersAsync()
        {
            return await _context.Users
                .Include(u => u.Role)
                .ToListAsync();
        }

        public async Task<User?> GetUserByIdAsync(int id)
        {
            return await _context.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.Id == id);
        }

        public async Task UpdateUserAsync(int id, User user)
        {
            var existingUser = await _context.Users
                .FirstOrDefaultAsync(u => u.Id == id);

            if (existingUser == null)
                throw new ArgumentException("User not found");

            existingUser.UserName = user.UserName;
            existingUser.Email = user.Email;
            existingUser.PasswordHash = user.PasswordHash;
            existingUser.RoleId = user.RoleId;
        }

        public async Task DeleteUserAsync(int id)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Id == id);

            if (user == null)
                throw new ArgumentException("User not found");

            _context.Users.Remove(user);
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
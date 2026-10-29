using QuizMaster.Domain.Entities;

namespace QuizMaster.Service.Interfaces.Repositories
{
    public interface IUserRepository
    {
        Task AddUserAsync(User user);
        Task<ICollection<User>> GetAllUsersAsync();
        Task<User?> GetUserByIdAsync(int id);
        Task UpdateUserAsync(int id, User user);
        Task DeleteUserAsync(int id);
        Task SaveChangesAsync();
    }
}
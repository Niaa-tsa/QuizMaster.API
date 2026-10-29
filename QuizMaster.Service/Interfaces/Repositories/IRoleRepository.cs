using QuizMaster.Domain.Entities;

namespace QuizMaster.Service.Interfaces.Repositories
{
    public interface IRoleRepository
    {
        Task AddRoleAsync(Role role);
        Task<ICollection<Role>> GetAllRolesAsync();
        Task<Role?> GetRoleByIdAsync(int id);
        Task UpdateRoleAsync(int id, Role role);
        Task DeleteRoleAsync(int id);
        Task SaveChangesAsync();
    }
}
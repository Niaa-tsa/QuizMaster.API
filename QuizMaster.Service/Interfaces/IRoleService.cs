using System.Collections.Generic;
using System.Threading.Tasks;
using QuizMaster.Domain.Entities;

namespace QuizMaster.Service.Interfaces
{
    public interface IRoleService
    {
        Task<List<Role>> GetAllAsync();
        Task<Role?> GetByIdAsync(int id);
        Task AddAsync(Role role);
        Task<bool> UpdateAsync(Role role);
        Task<bool> DeleteAsync(int id);
    }
}
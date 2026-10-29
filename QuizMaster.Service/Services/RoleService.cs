
using QuizMaster.Domain.Entities;
using QuizMaster.Service.Interfaces;
using QuizMaster.Service.Interfaces.Repositories;

namespace QuizMaster.Service.Services;

public class RoleService : IRoleService
{
    private readonly IRoleRepository _repository;

    public RoleService(IRoleRepository repository)
    {
        _repository = repository;
    }

    public async Task<List<Role>> GetAllAsync()
    {
        var roles = await _repository.GetAllRolesAsync();
        return roles.ToList();
    }

    public Task<Role?> GetByIdAsync(int id)
    {
        return _repository.GetRoleByIdAsync(id);
    }

    public async Task AddAsync(Role role)
    {
        await _repository.AddRoleAsync(role);
        await _repository.SaveChangesAsync();
    }

    public async Task<bool> UpdateAsync(Role role)
    {
        var existingRole = await _repository.GetRoleByIdAsync(role.Id);

        if (existingRole is null)
            return false;

        await _repository.UpdateRoleAsync(role.Id, role);
        await _repository.SaveChangesAsync();

        return true;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var role = await _repository.GetRoleByIdAsync(id);

        if (role is null)
            return false;

        await _repository.DeleteRoleAsync(id);
        await _repository.SaveChangesAsync();

        return true;
    }
}

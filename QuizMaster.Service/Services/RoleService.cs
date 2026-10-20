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

    public Task<List<Role>> GetAllAsync() =>
        _repository.GetAllAsync();

    public Task<Role?> GetByIdAsync(int id) =>
        _repository.GetByIdAsync(id);

    public async Task AddAsync(Role role)
    {
        await _repository.AddAsync(role);
        await _repository.SaveChangesAsync();
    }

    public async Task<bool> UpdateAsync(Role role)
    {
        if (await _repository.GetByIdAsync(role.Id) is null)
            return false;

        _repository.Update(role);
        await _repository.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var role = await _repository.GetByIdAsync(id);

        if (role is null)
            return false;

        _repository.Delete(role);
        await _repository.SaveChangesAsync();
        return true;
    }
}
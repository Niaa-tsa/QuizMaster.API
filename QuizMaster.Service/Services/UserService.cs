
using QuizMaster.Domain.Entities;
using QuizMaster.Service.Interfaces;
using QuizMaster.Service.Interfaces.Repositories;

namespace QuizMaster.Service.Services;

public class UserService : IUserService
{
    private readonly IUserRepository _repository;

    public UserService(IUserRepository repository)
    {
        _repository = repository;
    }

    public async Task<List<User>> GetAllAsync()
    {
        var users = await _repository.GetAllUsersAsync();
        return users.ToList();
    }

    public Task<User?> GetByIdAsync(int id)
    {
        return _repository.GetUserByIdAsync(id);
    }

    public async Task AddAsync(User user)
    {
        await _repository.AddUserAsync(user);
        await _repository.SaveChangesAsync();
    }

    public async Task<bool> UpdateAsync(User user)
    {
        var existingUser = await _repository.GetUserByIdAsync(user.Id);

        if (existingUser is null)
            return false;

        await _repository.UpdateUserAsync(user.Id, user);
        await _repository.SaveChangesAsync();

        return true;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var user = await _repository.GetUserByIdAsync(id);

        if (user is null)
            return false;

        await _repository.DeleteUserAsync(id);
        await _repository.SaveChangesAsync();

        return true;
    }
}

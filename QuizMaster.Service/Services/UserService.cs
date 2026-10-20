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

    public Task<List<User>> GetAllAsync() =>
        _repository.GetAllAsync();

    public Task<User?> GetByIdAsync(int id) =>
        _repository.GetByIdAsync(id);

    public async Task AddAsync(User user)
    {
        await _repository.AddAsync(user);
        await _repository.SaveChangesAsync();
    }

    public async Task<bool> UpdateAsync(User user)
    {
        if (await _repository.GetByIdAsync(user.Id) is null)
            return false;

        _repository.Update(user);
        await _repository.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var user = await _repository.GetByIdAsync(id);

        if (user is null)
            return false;

        _repository.Delete(user);
        await _repository.SaveChangesAsync();
        return true;
    }
}
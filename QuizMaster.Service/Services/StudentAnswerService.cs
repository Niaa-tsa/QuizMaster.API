using QuizMaster.Domain.Entities;
using QuizMaster.Service.Interfaces;
using QuizMaster.Service.Interfaces.Repositories;

namespace QuizMaster.Service.Services;

public class StudentAnswerService : IStudentAnswerService
{
    private readonly IStudentAnswerRepository _repository;

    public StudentAnswerService(IStudentAnswerRepository repository)
    {
        _repository = repository;
    }

    public Task<List<StudentAnswer>> GetAllAsync() =>
        _repository.GetAllAsync();

    public Task<StudentAnswer?> GetByIdAsync(int id) =>
        _repository.GetByIdAsync(id);

    public async Task AddAsync(StudentAnswer studentAnswer)
    {
        await _repository.AddAsync(studentAnswer);
        await _repository.SaveChangesAsync();
    }

    public async Task<bool> UpdateAsync(StudentAnswer studentAnswer)
    {
        if (await _repository.GetByIdAsync(studentAnswer.Id) is null)
            return false;

        _repository.Update(studentAnswer);
        await _repository.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var studentAnswer = await _repository.GetByIdAsync(id);

        if (studentAnswer is null)
            return false;

        _repository.Delete(studentAnswer);
        await _repository.SaveChangesAsync();
        return true;
    }
}
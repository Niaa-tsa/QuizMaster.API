
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

    public async Task<List<StudentAnswer>> GetAllAsync()
    {
        var answers = await _repository.GetAllStudentAnswersAsync();
        return answers.ToList();
    }

    public Task<StudentAnswer?> GetByIdAsync(int id)
    {
        return _repository.GetStudentAnswerByIdAsync(id);
    }

    public async Task AddAsync(StudentAnswer studentAnswer)
    {
        await _repository.AddStudentAnswerAsync(studentAnswer);
        await _repository.SaveChangesAsync();
    }

    public async Task<bool> UpdateAsync(StudentAnswer studentAnswer)
    {
        var existingAnswer =
            await _repository.GetStudentAnswerByIdAsync(studentAnswer.Id);

        if (existingAnswer is null)
            return false;

        await _repository.UpdateStudentAnswerAsync(
            studentAnswer.Id, studentAnswer);

        await _repository.SaveChangesAsync();

        return true;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var studentAnswer =
            await _repository.GetStudentAnswerByIdAsync(id);

        if (studentAnswer is null)
            return false;

        await _repository.DeleteStudentAnswerAsync(id);
        await _repository.SaveChangesAsync();

        return true;
    }
}

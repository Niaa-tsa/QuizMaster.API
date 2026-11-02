using QuizMaster.Domain.Entities;

namespace QuizMaster.Service.Interfaces.Repositories;

public interface IQuizResultRepository
{
    Task AddQuizResultAsync(QuizResult quizResult);
    Task SaveChangesAsync();
}

using QuizMaster.Domain.Entities;

namespace QuizMaster.Service.Interfaces.Repositories
{
    public interface IStudentAnswerRepository
    {
        Task AddStudentAnswerAsync(StudentAnswer studentAnswer);

        Task<ICollection<StudentAnswer>> GetAnswersByAttemptAndQuestionAsync(
            int attemptId,
            int questionId);

        Task<bool> HasAnswersForOptionAsync(int answerOptionId);

        Task<bool> HasAnswersForQuestionAsync(int questionId);

        Task SaveChangesAsync();
    }
}
using QuizMaster.Domain.DTOs.Attempts;

namespace QuizMaster.Service.Interfaces;

public interface IStudentAnswerService
{
    Task<bool> SubmitAnswerAsync(
        int attemptId,
        int userId,
        SubmitAnswerDTO dto);
}
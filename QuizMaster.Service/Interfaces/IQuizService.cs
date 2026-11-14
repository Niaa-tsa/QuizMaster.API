using QuizMaster.Domain.DTOs.Quizzes;
using QuizMaster.Domain.Entities;

namespace QuizMaster.Service.Interfaces;

public interface IQuizService
{
    Task<List<QuizResponseDTO>> GetPublishedAsync();

    Task<List<QuizResponseDTO>> GetByLecturerAsync(int lecturerId);
    Task<QuizResponseDTO> GetOwnedAsync(int quizId, int lecturerId);
    Task<QuizResponseDTO> CreateAsync(CreateQuizDTO dto, int lecturerId);
    Task<QuizResponseDTO> UpdateAsync(int quizId, UpdateQuizDTO dto, int lecturerId);
    Task DeleteAsync(int quizId, int lecturerId);
    Task PublishAsync(int quizId, int lecturerId);
    Task CloseAsync(int quizId, int lecturerId);

    Task<Quiz> GetOwnedQuizAsync(int quizId, int lecturerId);
    Task<Quiz> GetEditableQuizAsync(int quizId, int lecturerId);
}
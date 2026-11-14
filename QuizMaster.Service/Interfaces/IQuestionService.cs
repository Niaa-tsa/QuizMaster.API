using QuizMaster.Domain.DTOs.Questions;
using QuizMaster.Domain.Entities;

namespace QuizMaster.Service.Interfaces;

public interface IQuestionService
{
    Task<Question?> GetByIdAsync(int id);

    Task<List<QuestionResponseDTO>> GetByLecturerAsync(int lecturerId);
    Task<QuestionResponseDTO> GetOwnedAsync(int questionId, int lecturerId);
    Task<QuestionResponseDTO> CreateAsync(int quizId, CreateQuestionDTO dto, int lecturerId);
    Task UpdateAsync(int questionId, UpdateQuestionDTO dto, int lecturerId);
    Task DeleteAsync(int questionId, int lecturerId);
}
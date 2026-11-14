using QuizMaster.Domain.DTOs.Questions;

namespace QuizMaster.Service.Interfaces;

public interface IAnswerOptionService
{
    Task<List<AnswerOptionDetailsDTO>> GetByQuestionAsync(int questionId, int lecturerId);
    Task<AnswerOptionDetailsDTO> CreateAsync(int questionId, AnswerOptionDTO dto, int lecturerId);
    Task UpdateAsync(int answerOptionId, AnswerOptionDTO dto, int lecturerId);
    Task DeleteAsync(int answerOptionId, int lecturerId);
}
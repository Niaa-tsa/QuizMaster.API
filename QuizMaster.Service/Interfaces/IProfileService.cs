using QuizMaster.Domain.DTOs.Profile;

namespace QuizMaster.Service.Interfaces;

public interface IProfileService
{
    Task<ProfileResponseDTO> GetAsync(int userId);
    Task<ProfileResponseDTO> UploadPhotoAsync(int userId, Stream content);
    Task DeletePhotoAsync(int userId);
}
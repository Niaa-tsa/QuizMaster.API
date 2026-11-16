using QuizMaster.Domain.DTOs.Auth;

namespace QuizMaster.Service.Interfaces;

public interface IAuthService
{
    Task<AuthResponseDTO> RegisterAsync(
        RegisterRequestDTO request);

    Task<AuthResponseDTO?> LoginAsync(
        LoginRequestDTO request);

    Task<bool> ChangePasswordAsync(
        int userId,
        ChangePasswordDTO request);
}
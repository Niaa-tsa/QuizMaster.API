using QuizMaster.Domain.DTOs.Admin;

namespace QuizMaster.Service.Interfaces;

public interface IAdminService
{
    Task<ICollection<AdminUserDTO>> GetAllUsersAsync();

    Task<ICollection<AdminUserDTO>> GetPendingLecturersAsync();

    Task<bool> ApproveLecturerAsync(int userId);

    Task<bool> RejectLecturerAsync(int userId);

    Task<bool> ChangeUserRoleAsync(
        int userId,
        ChangeUserRoleDTO dto);
}

using QuizMaster.Domain.DTOs.Admin;

namespace QuizMaster.Service.Interfaces;

public interface ISuperAdminService
{
    // Admin Management
    Task<bool> CreateAdminAsync(CreateAdminDTO dto);

    Task<List<AdminUserDTO>> GetAllAdminsAsync();

    Task<AdminUserDTO?> GetAdminByIdAsync(int id);

    Task<bool> DeleteAdminAsync(int id);
}

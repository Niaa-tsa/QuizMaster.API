
using QuizMaster.Domain.DTOs.Admin;
using QuizMaster.Service.Interfaces;
using QuizMaster.Service.Interfaces.Repositories;

namespace QuizMaster.Service.Services;

public class AdminService : IAdminService
{
    private readonly IUserRepository _userRepository;
    private readonly IRoleRepository _roleRepository;
    public AdminService(
    IUserRepository userRepository,
    IRoleRepository roleRepository)
    {
        _userRepository = userRepository;
        _roleRepository = roleRepository;
    }

    public async Task<ICollection<AdminUserDTO>> GetAllUsersAsync()
    {
        var users = await _userRepository.GetAllUsersAsync();

        return users.Select(user => new AdminUserDTO
        {
            Id = user.Id,
            UserName = user.UserName,
            Email = user.Email,
            Role = user.Role.Name,
            RequestedRole = user.RequestedRole,
            ApprovalStatus = user.ApprovalStatus
        }).ToList();
    }

    public async Task<ICollection<AdminUserDTO>> GetPendingLecturersAsync()
    {
        var users = await _userRepository.GetAllUsersAsync();

        return users
            .Where(user =>
                user.RequestedRole == "Lecturer" &&
                user.ApprovalStatus == "Pending")
            .Select(user => new AdminUserDTO
            {
                Id = user.Id,
                UserName = user.UserName,
                Email = user.Email,
                Role = user.Role.Name,
                RequestedRole = user.RequestedRole,
                ApprovalStatus = user.ApprovalStatus
            })
            .ToList();
    }


    public async Task<bool> ApproveLecturerAsync(int userId)
    {
        var user = await _userRepository.GetUserByIdAsync(userId);

        if (user == null)
            return false;

        if (user.RequestedRole != "Lecturer" ||
            user.ApprovalStatus != "Pending")
        {
            return false;
        }

        var roles = await _roleRepository.GetAllRolesAsync();

        var lecturerRole = roles.FirstOrDefault(r =>
            r.Name.Equals(
                "Lecturer",
                StringComparison.OrdinalIgnoreCase));

        if (lecturerRole == null)
            throw new InvalidOperationException(
                "Lecturer role was not found.");

        user.RoleId = lecturerRole.Id;

        user.RequestedRole = null;
        user.ApprovalStatus = "Approved";

        await _userRepository.SaveChangesAsync();

        return true;
    }

    public async Task<bool> RejectLecturerAsync(int userId)
    {
        var user = await _userRepository.GetUserByIdAsync(userId);

        if (user == null)
            return false;

        if (user.RequestedRole != "Lecturer" ||
            user.ApprovalStatus != "Pending")
        {
            return false;
        }

        user.RequestedRole = null;
        user.ApprovalStatus = "Rejected";

        await _userRepository.SaveChangesAsync();

        return true;
    }
    public async Task<bool> ChangeUserRoleAsync(
       int userId,
       ChangeUserRoleDTO dto)
    {
        if (dto.Role == null ||
            (!dto.Role.Equals("Student",
                StringComparison.OrdinalIgnoreCase) &&
             !dto.Role.Equals("Lecturer",
                StringComparison.OrdinalIgnoreCase)))
        {
            return false;
        }

        var user = await _userRepository.GetUserByIdAsync(userId);

        if (user == null)
            return false;

        if (user.Role.Name.Equals(
         "Admin",
         StringComparison.OrdinalIgnoreCase) ||
     user.Role.Name.Equals(
         "SuperAdmin",
         StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        var roles = await _roleRepository.GetAllRolesAsync();

        var newRole = roles.FirstOrDefault(r =>
            r.Name.Equals(
                dto.Role,
                StringComparison.OrdinalIgnoreCase));

        if (newRole == null)
            return false;

        user.RoleId = newRole.Id;

        user.RequestedRole = null;
        user.ApprovalStatus = "Approved";

        await _userRepository.SaveChangesAsync();

        return true;
    }

}


using Microsoft.AspNetCore.Identity;
using QuizMaster.Domain.DTOs.Admin;
using QuizMaster.Domain.Entities;
using QuizMaster.Service.Interfaces;
using QuizMaster.Service.Interfaces.Repositories;

namespace QuizMaster.Service.Services;

public class SuperAdminService : ISuperAdminService
{
    private readonly IUserRepository _userRepository;
    private readonly IRoleRepository _roleRepository;
    private readonly IPasswordHasher<User> _passwordHasher;

    public SuperAdminService(
        IUserRepository userRepository,
        IRoleRepository roleRepository,
        IPasswordHasher<User> passwordHasher)
    {
        _userRepository = userRepository;
        _roleRepository = roleRepository;
        _passwordHasher = passwordHasher;
    }

    // CREATE ADMIN
    public async Task<bool> CreateAdminAsync(CreateAdminDTO dto)
    {
        if (string.IsNullOrWhiteSpace(dto.UserName) ||
            string.IsNullOrWhiteSpace(dto.Email) ||
            string.IsNullOrWhiteSpace(dto.Password))
        {
            return false;
        }

        var existingUser =
            await _userRepository.GetUserByEmailAsync(dto.Email.Trim());

        if (existingUser != null)
            return false;

        var roles = await _roleRepository.GetAllRolesAsync();

        var adminRole = roles.FirstOrDefault(r =>
            r.Name.Equals("Admin",
                StringComparison.OrdinalIgnoreCase));

        if (adminRole == null)
            throw new InvalidOperationException(
                "Admin role was not found.");

        var user = new User
        {
            UserName = dto.UserName.Trim(),
            Email = dto.Email.Trim(),
            RoleId = adminRole.Id,
            ApprovalStatus = "Approved"
        };

        user.PasswordHash =
            _passwordHasher.HashPassword(user, dto.Password);

        await _userRepository.AddUserAsync(user);
        await _userRepository.SaveChangesAsync();

        return true;
    }

    // GET ALL ADMINS
    public async Task<List<AdminUserDTO>> GetAllAdminsAsync()
    {
        var users = await _userRepository.GetAllUsersAsync();

        return users
            .Where(u => u.Role?.Name == "Admin")
            .Select(u => new AdminUserDTO
            {
                Id = u.Id,
                UserName = u.UserName,
                Email = u.Email,
                Role = u.Role!.Name,
                RequestedRole = u.RequestedRole,
                ApprovalStatus = u.ApprovalStatus
            })
            .ToList();
    }

    // GET ADMIN BY ID
    public async Task<AdminUserDTO?> GetAdminByIdAsync(int id)
    {
        var user = await _userRepository.GetUserByIdAsync(id);

        if (user == null || user.Role?.Name != "Admin")
            return null;

        return new AdminUserDTO
        {
            Id = user.Id,
            UserName = user.UserName,
            Email = user.Email,
            Role = user.Role.Name,
            RequestedRole = user.RequestedRole,
            ApprovalStatus = user.ApprovalStatus
        };
    }

    // DELETE ADMIN
    public async Task<bool> DeleteAdminAsync(int id)
    {
        var user = await _userRepository.GetUserByIdAsync(id);

        if (user == null || user.Role?.Name != "Admin")
            return false;

        await _userRepository.DeleteUserAsync(id);
        await _userRepository.SaveChangesAsync();

        return true;
    }
}


using Microsoft.AspNetCore.Identity;
using QuizMaster.Domain.DTOs.Auth;
using QuizMaster.Domain.Entities;
using QuizMaster.Service.Exceptions;
using QuizMaster.Service.Interfaces;
using QuizMaster.Service.Interfaces.Repositories;

namespace QuizMaster.Service.Services;

public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly IRoleRepository _roleRepository;
    private readonly IPasswordHasher<User> _passwordHasher;
    private readonly IJwtTokenService _jwtTokenService;

    public AuthService(
        IUserRepository userRepository,
        IRoleRepository roleRepository,
        IPasswordHasher<User> passwordHasher,
        IJwtTokenService jwtTokenService)
    {
        _userRepository = userRepository;
        _roleRepository = roleRepository;
        _passwordHasher = passwordHasher;
        _jwtTokenService = jwtTokenService;
    }

    // REGISTER

    public async Task<AuthResponseDTO> RegisterAsync(
        RegisterRequestDTO request)
    {
        var email = request.Email.Trim();

        var existingUser = await _userRepository
            .GetUserByEmailAsync(email);

        if (existingUser != null)
        {
            throw new ConflictException(
       "An account with this email already exists.");
        }

        var requestedRole = request.RequestedRole.Trim();

        if (!requestedRole.Equals("Student",
                StringComparison.OrdinalIgnoreCase) &&
            !requestedRole.Equals("Lecturer",
                StringComparison.OrdinalIgnoreCase))
        {
            throw new BusinessRuleException(
    "Only Student and Lecturer registration is allowed.");
        }

        var roles = await _roleRepository.GetAllRolesAsync();

        var studentRole = roles.FirstOrDefault(r =>
            r.Name.Equals("Student",
                StringComparison.OrdinalIgnoreCase));

        if (studentRole is null)
        {
            throw new InvalidOperationException(
                "Student role was not found.");
        }

        bool isLecturer = requestedRole.Equals(
            "Lecturer",
            StringComparison.OrdinalIgnoreCase);

        var user = new User
        {
            UserName = request.UserName.Trim(),
            Email = email,
            RoleId = studentRole.Id,

            RequestedRole = isLecturer ? "Lecturer" : null,

            ApprovalStatus = isLecturer
                ? "Pending"
                : "Approved"
        };

        user.PasswordHash =
            _passwordHasher.HashPassword(user, request.Password);

        await _userRepository.AddUserAsync(user);
        await _userRepository.SaveChangesAsync();

        user.Role = studentRole;

        if (isLecturer)
        {
            return new AuthResponseDTO
            {
                UserId = user.Id,
                UserName = user.UserName,
                Email = user.Email,
                Role = "Pending",
                Token = string.Empty
            };
        }

        return CreateAuthResponse(user);
    }

    // LOGIN

    public async Task<AuthResponseDTO?> LoginAsync(
        LoginRequestDTO request)
    {
        var user = await _userRepository.GetUserByEmailAsync(
            request.Email.Trim());

        if (user is null)
            return null;

        var verificationResult =
            _passwordHasher.VerifyHashedPassword(
                user,
                user.PasswordHash,
                request.Password);

        if (verificationResult == PasswordVerificationResult.Failed)
            return null;

        
        if (user.ApprovalStatus != "Approved")
            return null;

        return CreateAuthResponse(user);
    }


    // CREATE AUTH RESPONSE
    private AuthResponseDTO CreateAuthResponse(User user)
    {
        var roleName = user.Role?.Name
            ?? throw new InvalidOperationException(
                "User role is missing.");

        return new AuthResponseDTO
        {
            UserId = user.Id,
            UserName = user.UserName,
            Email = user.Email,
            Role = roleName,
            Token = _jwtTokenService.GenerateToken(user)
        };
    }
}

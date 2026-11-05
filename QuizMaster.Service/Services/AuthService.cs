using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using QuizMaster.Domain.DTOs.Auth;
using QuizMaster.Domain.Entities;
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

    public async Task<AuthResponseDTO> RegisterAsync(
        RegisterRequestDTO request)
    {
        var existingUser = await _userRepository.GetUserByEmailAsync(
            request.Email.Trim());

        if (existingUser != null)
        {
            throw new InvalidOperationException(
                "An account with this email already exists.");
        }

        var roles = await _roleRepository.GetAllRolesAsync();

        // თუ მოთხოვნაში როლი მოჰყვება, ვიყენებთ მას, წინააღმდეგ შემთხვევაში ვტოვებთ Student-ს
        var requestedRoleName = !string.IsNullOrEmpty(request.Role) ? request.Role : "Student";

        var targetRole = roles.FirstOrDefault(r =>
            r.Name.Equals(requestedRoleName,
                StringComparison.OrdinalIgnoreCase));

        if (targetRole is null)
        {
            throw new InvalidOperationException(
                $"Role '{requestedRoleName}' was not found in the database.");
        }

        var user = new User
        {
            UserName = request.UserName.Trim(),
            Email = request.Email.Trim(),
            RoleId = targetRole.Id
        };

        user.PasswordHash =
            _passwordHasher.HashPassword(user, request.Password);

        await _userRepository.AddUserAsync(user);
        await _userRepository.SaveChangesAsync();

        user.Role = targetRole;

        return CreateAuthResponse(user);
    }

    public async Task<AuthResponseDTO?> LoginAsync(
        LoginRequestDTO request)
    {
        var user = await _userRepository.GetUserByEmailAsync(
            request.Email.Trim());

        if (user is null)
            return null;

        var verificationResult =
            _passwordHasher.VerifyHashedPassword(
                user, user.PasswordHash, request.Password);

        if (verificationResult == PasswordVerificationResult.Failed)
            return null;

        return CreateAuthResponse(user);
    }

    private AuthResponseDTO CreateAuthResponse(User user)
    {
        var roleName = user.Role?.Name
            ?? throw new InvalidOperationException("User role is missing.");

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
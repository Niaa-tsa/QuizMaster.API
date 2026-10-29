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
    private readonly IConfiguration _configuration;

    public AuthService(
        IUserRepository userRepository,
        IRoleRepository roleRepository,
        IPasswordHasher<User> passwordHasher,
        IConfiguration configuration)
    {
        _userRepository = userRepository;
        _roleRepository = roleRepository;
        _passwordHasher = passwordHasher;
        _configuration = configuration;
    }

    public async Task<AuthResponseDTO> RegisterAsync(
        RegisterRequestDTO request)
    {
        var users = await _userRepository.GetAllUsersAsync();

        if (users.Any(u =>
            u.Email.Equals(request.Email.Trim(),
                StringComparison.OrdinalIgnoreCase)))
        {
            throw new InvalidOperationException(
                "An account with this email already exists.");
        }

        var roles = await _roleRepository.GetAllRolesAsync();

        var studentRole = roles.FirstOrDefault(r =>
            r.Name.Equals("Student",
                StringComparison.OrdinalIgnoreCase));

        if (studentRole is null)
        {
            throw new InvalidOperationException(
                "Student role was not found in the database.");
        }

        var user = new User
        {
            UserName = request.UserName.Trim(),
            Email = request.Email.Trim(),
            RoleId = studentRole.Id
        };

        user.PasswordHash =
            _passwordHasher.HashPassword(user, request.Password);

        await _userRepository.AddUserAsync(user);
        await _userRepository.SaveChangesAsync();

        user.Role = studentRole;

        return CreateAuthResponse(user);
    }

    public async Task<AuthResponseDTO?> LoginAsync(
        LoginRequestDTO request)
    {
        var users = await _userRepository.GetAllUsersAsync();

        var user = users.FirstOrDefault(u =>
            u.Email.Equals(request.Email.Trim(),
                StringComparison.OrdinalIgnoreCase));

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
        var key = _configuration["Jwt:Key"]
            ?? throw new InvalidOperationException("JWT Key is missing.");

        var issuer = _configuration["Jwt:Issuer"]
            ?? throw new InvalidOperationException("JWT Issuer is missing.");

        var audience = _configuration["Jwt:Audience"]
            ?? throw new InvalidOperationException("JWT Audience is missing.");

        var expirationMinutes = int.Parse(
            _configuration["Jwt:ExpirationMinutes"] ?? "60");

        var roleName = user.Role?.Name
            ?? throw new InvalidOperationException("User role is missing.");

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Name, user.UserName),
            new(ClaimTypes.Email, user.Email),
            new(ClaimTypes.Role, roleName)
        };

        var signingKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(key));

        var credentials = new SigningCredentials(
            signingKey, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(expirationMinutes),
            signingCredentials: credentials);

        return new AuthResponseDTO
        {
            UserId = user.Id,
            UserName = user.UserName,
            Email = user.Email,
            Role = roleName,
            Token = new JwtSecurityTokenHandler().WriteToken(token)
        };
    }
}
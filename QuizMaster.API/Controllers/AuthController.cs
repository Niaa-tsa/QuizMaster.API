using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuizMaster.API.Extensions;
using QuizMaster.Domain.DTOs.Auth;
using QuizMaster.Service.Interfaces;

namespace QuizMaster.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly ILogger<AuthController> _logger;

    public AuthController(
        IAuthService authService,
        ILogger<AuthController> logger)
    {
        _authService = authService;
        _logger = logger;
    }

    /// <summary>
    /// არეგისტრირებს მომხმარებელს.
    /// </summary>
    [HttpPost("register")]
    public async Task<IActionResult> Register(
        RegisterRequestDTO request)
    {
        var response = await _authService.RegisterAsync(request);

        _logger.LogInformation(
            "New user registered: {Email}", request.Email);

        return Ok(response);
    }

    /// <summary>
    /// ახდენს მომხმარებლის ავტორიზაციას და აბრუნებს JWT ტოკენს.
    /// </summary>
    [HttpPost("login")]
    public async Task<IActionResult> Login(
        LoginRequestDTO request)
    {
        var response = await _authService.LoginAsync(request);

        if (response is null)
        {
            _logger.LogWarning(
                "Failed login attempt for {Email}", request.Email);

            return Problem(statusCode: 401, title: "Invalid email or password.");
        }

        _logger.LogInformation(
            "User logged in: {Email}", request.Email);

        return Ok(response);
    }
    /// <summary>
    /// ცვლის პაროლს.
    /// </summary>
    [Authorize]
    [HttpPost("change-password")]
    public async Task<IActionResult> ChangePassword(
        [FromBody] ChangePasswordDTO request)
    {
        var userId = User.GetUserId();

        var result = await _authService.ChangePasswordAsync(
            userId,
            request);

        if (!result)
        {
            return Problem(
                statusCode: 400,
                title: "Incorrect current password or user not found.");
        }

        return Ok(new
        {
            message = "Password changed successfully!"
        });
    }
}
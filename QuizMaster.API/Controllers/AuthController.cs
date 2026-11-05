using Microsoft.AspNetCore.Mvc;
using QuizMaster.Domain.DTOs.Auth;
using QuizMaster.Service.Interfaces;

namespace QuizMaster.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }
    /// <summary>
    /// დარეგისტრირებას უკეთებს ახალ სტუდენტს სისტემაში.
    /// </summary>
    [HttpPost("register")]
    public async Task<IActionResult> Register(
        RegisterRequestDTO request)
    {
        try
        {
            var response = await _authService.RegisterAsync(request);
            return Ok(response);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
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
            return Unauthorized(new
            {
                message = "Invalid email or password."
            });
        }

        return Ok(response);
    }
}
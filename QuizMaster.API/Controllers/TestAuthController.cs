using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace QuizMaster.API.Controllers;

/// <summary>
/// ამოწმებს მომხმარებლის ავტორიზაციას.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class TestAuthController : ControllerBase
{
    /// <summary>
    /// ამოწმებს სტუდენტის ავტორიზაციას.
    /// </summary>
    [HttpGet("student")]
    [Authorize(Roles = "Student")]
    public IActionResult StudentOnly()
    {
        return Ok(new
        {
            message = "Student authorization successful!",
            userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value,
            userName = User.Identity?.Name,
            role = User.FindFirst(ClaimTypes.Role)?.Value
        });
    }

    /// <summary>
    /// ამოწმებს ლექტორის ავტორიზაციას.
    /// </summary>
    [HttpGet("lecturer")]
    [Authorize(Roles = "Lecturer")]
    public IActionResult LecturerOnly()
    {
        return Ok(new
        {
            message = "Lecturer authorization successful!"
        });
    }
}
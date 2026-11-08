
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuizMaster.Service.Services;

namespace QuizMaster.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
public class AdminController : ControllerBase
{
    private readonly AdminService _adminService;

    public AdminController(AdminService adminService)
    {
        _adminService = adminService;
    }

    /// <summary>
    /// აბრუნებს სისტემაში რეგისტრირებულ ყველა მომხმარებელს.
    /// </summary>
    [HttpGet("users")]
    public async Task<IActionResult> GetAllUsers()
    {
        var users = await _adminService.GetAllUsersAsync();

        return Ok(users);
    }

    /// <summary>
    /// აბრუნებს Lecturer-ის დადასტურების მომლოდინე მოთხოვნებს.
    /// </summary>
    [HttpGet("pending-lecturers")]
    public async Task<IActionResult> GetPendingLecturers()
    {
        var users = await _adminService.GetPendingLecturersAsync();

        return Ok(users);
    }

    /// <summary>
    /// ადასტურებს მომხმარებლის Lecturer-ის მოთხოვნას.
    /// </summary>
    [HttpPost("approve-lecturer/{userId}")]
    public async Task<IActionResult> ApproveLecturer(int userId)
    {
        var result = await _adminService.ApproveLecturerAsync(userId);

        if (!result)
        {
            return BadRequest(
                "User not found or lecturer request is not pending.");
        }

        return Ok(new
        {
            message = "Lecturer approved successfully!"
        });
    }

    /// <summary>
    /// უარყოფს მომხმარებლის Lecturer-ის მოთხოვნას.
    /// </summary>
    [HttpPost("reject-lecturer/{userId}")]
    public async Task<IActionResult> RejectLecturer(int userId)
    {
        var result = await _adminService.RejectLecturerAsync(userId);

        if (!result)
        {
            return BadRequest(
                "User not found or lecturer request is not pending.");
        }

        return Ok(new
        {
            message = "Lecturer request rejected successfully!"
        });
    }

}

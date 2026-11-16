using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuizMaster.Domain.DTOs.Admin;
using QuizMaster.Service.Interfaces;

namespace QuizMaster.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "SuperAdmin")]
public class SuperAdminController : ControllerBase
{
    private readonly ISuperAdminService _superAdminService;
    private readonly IAdminService _adminService;

    public SuperAdminController(
        ISuperAdminService superAdminService,
        IAdminService adminService)
    {
        _superAdminService = superAdminService;
        _adminService = adminService;
    }

    // =====================================
    // ADMIN MANAGEMENT
    // =====================================

    // CREATE ADMIN
    [HttpPost("create-admin")]
    public async Task<IActionResult> CreateAdmin(
        [FromBody] CreateAdminDTO dto)
    {
        var result = await _superAdminService.CreateAdminAsync(dto);

        if (!result)
        {
            return Problem(
                statusCode: 400,
                title: "Invalid data or email already exists.");
        }

        return Ok(new
        {
            message = "Admin created successfully!"
        });
    }

    // GET ALL ADMINS
    [HttpGet("admins")]
    public async Task<IActionResult> GetAllAdmins()
    {
        var admins = await _superAdminService.GetAllAdminsAsync();

        return Ok(admins);
    }

    // GET ADMIN BY ID
    [HttpGet("admins/{id}")]
    public async Task<IActionResult> GetAdminById(int id)
    {
        var admin = await _superAdminService.GetAdminByIdAsync(id);

        if (admin == null)
        {
            return Problem(
                statusCode: 404,
                title: "Admin not found.");
        }

        return Ok(admin);
    }

    // DELETE ADMIN
    [HttpDelete("admins/{id}")]
    public async Task<IActionResult> DeleteAdmin(int id)
    {
        var result = await _superAdminService.DeleteAdminAsync(id);

        if (!result)
        {
            return Problem(
                statusCode: 404,
                title: "Admin not found.");
        }

        return Ok(new
        {
            message = "Admin deleted successfully!"
        });
    }

    // =====================================
    // ADMIN FUNCTIONS
    // =====================================

    // GET ALL USERS
    [HttpGet("users")]
    public async Task<IActionResult> GetAllUsers()
    {
        var users = await _adminService.GetAllUsersAsync();

        return Ok(users);
    }

    // GET PENDING LECTURERS
    [HttpGet("pending-lecturers")]
    public async Task<IActionResult> GetPendingLecturers()
    {
        var users = await _adminService.GetPendingLecturersAsync();

        return Ok(users);
    }

    // APPROVE LECTURER
    [HttpPost("approve-lecturer/{userId}")]
    public async Task<IActionResult> ApproveLecturer(int userId)
    {
        var result = await _adminService.ApproveLecturerAsync(userId);

        if (!result)
        {
            return Problem(
                statusCode: 400,
                title: "User not found or lecturer request is not pending.");
        }

        return Ok(new
        {
            message = "Lecturer approved successfully!"
        });
    }

    // REJECT LECTURER
    [HttpPost("reject-lecturer/{userId}")]
    public async Task<IActionResult> RejectLecturer(int userId)
    {
        var result = await _adminService.RejectLecturerAsync(userId);

        if (!result)
        {
            return Problem(
                statusCode: 400,
                title: "User not found or lecturer request is not pending.");
        }

        return Ok(new
        {
            message = "Lecturer request rejected successfully!"
        });
    }

    // CHANGE USER ROLE
    [HttpPut("change-role/{userId}")]
    public async Task<IActionResult> ChangeUserRole(
        int userId,
        [FromBody] ChangeUserRoleDTO dto)
    {
        var result = await _adminService.ChangeUserRoleAsync(
            userId,
            dto);

        if (!result)
        {
            return Problem(
                statusCode: 400,
                title: "User not found or invalid role.");
        }

        return Ok(new
        {
            message = "User role changed successfully!"
        });
    }
}
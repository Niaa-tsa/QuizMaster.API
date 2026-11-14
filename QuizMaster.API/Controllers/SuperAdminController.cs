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

    public SuperAdminController(
        ISuperAdminService superAdminService)
    {
        _superAdminService = superAdminService;
    }

    // CREATE ADMIN
    [HttpPost("create-admin")]
    public async Task<IActionResult> CreateAdmin(
        [FromBody] CreateAdminDTO dto)
    {
        var result = await _superAdminService.CreateAdminAsync(dto);

        if (!result)
        {
            return Problem(statusCode: 400, title: "Invalid data or email already exists.");
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
            return Problem(statusCode: 404, title: "Admin not found.");

        return Ok(admin);
    }

    // DELETE ADMIN
    [HttpDelete("admins/{id}")]
    public async Task<IActionResult> DeleteAdmin(int id)
    {
        var result = await _superAdminService.DeleteAdminAsync(id);

        if (!result)
            return Problem(statusCode: 404, title: "Admin not found.");

        return Ok(new
        {
            message = "Admin deleted successfully!"
        });
    }
}
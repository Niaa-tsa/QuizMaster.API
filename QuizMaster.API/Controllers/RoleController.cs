using Microsoft.AspNetCore.Mvc;
using QuizMaster.Domain.Entities;
using QuizMaster.Service.Interfaces;
using Microsoft.AspNetCore.Authorization;

namespace QuizMaster.API.Controllers;

/// <summary>
/// მართავს მომხმარებლის როლებს.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "SuperAdmin")]
public class RoleController : ControllerBase
{
    private readonly IRoleService _service;

    public RoleController(IRoleService service)
    {
        _service = service;
    }

    /// <summary>
    /// აბრუნებს ყველა როლს.
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<List<Role>>> GetAll()
    {
        return Ok(await _service.GetAllAsync());
    }

    /// <summary>
    /// აბრუნებს კონკრეტულ როლს.
    /// </summary>
    [HttpGet("{id:int}")]
    public async Task<ActionResult<Role>> GetById(int id)
    {
        var role = await _service.GetByIdAsync(id);

        if (role is null)
            return NotFound();

        return Ok(role);
    }

    /// <summary>
    /// ამატებს ახალ როლს.
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> Create(Role role)
    {
        await _service.AddAsync(role);
        return Ok(role);
    }

    /// <summary>
    /// ანახლებს არსებულ როლს.
    /// </summary>
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, Role role)
    {
        if (id != role.Id)
            return BadRequest();

        var updated = await _service.UpdateAsync(role);

        if (!updated)
            return NotFound();

        return NoContent();
    }

    /// <summary>
    /// შლის არსებულ როლს.
    /// </summary>
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var deleted = await _service.DeleteAsync(id);

        if (!deleted)
            return NotFound();

        return NoContent();
    }
}
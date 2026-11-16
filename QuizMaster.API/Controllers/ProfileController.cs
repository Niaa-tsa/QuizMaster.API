using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuizMaster.API.Extensions;
using QuizMaster.Service.Interfaces;

namespace QuizMaster.API.Controllers;

/// <summary>
/// ავტორიზებული მომხმარებლის პროფილი და პროფილის ფოტო (ყველა როლისთვის).
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ProfileController : ControllerBase
{
    private readonly IProfileService _profileService;
    private readonly ILogger<ProfileController> _logger;

    public ProfileController(
        IProfileService profileService,
        ILogger<ProfileController> logger)
    {
        _profileService = profileService;
        _logger = logger;
    }

    /// <summary>
    /// აბრუნებს ავტორიზებული მომხმარებლის პროფილს.
    /// </summary>
    [HttpGet("me")]
    public async Task<IActionResult> GetMe()
    {
        var profile = await _profileService.GetAsync(User.GetUserId());

        return Ok(profile);
    }

    /// <summary>
    /// ტვირთავს პროფილის ფოტოს (JPEG, PNG ან WebP, მაქსიმუმ 2 MB).
    /// ახალი ფოტო ანაცვლებს ძველს.
    /// </summary>
    [HttpPost("photo")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> UploadPhoto(IFormFile file)
    {
        var userId = User.GetUserId();

        await using var stream = file.OpenReadStream();

        var profile = await _profileService.UploadPhotoAsync(userId, stream);

        _logger.LogInformation(
            "User {UserId} uploaded a profile photo", userId);

        return Ok(profile);
    }

    /// <summary>
    /// შლის პროფილის ფოტოს.
    /// </summary>
    [HttpDelete("photo")]
    public async Task<IActionResult> DeletePhoto()
    {
        var userId = User.GetUserId();

        await _profileService.DeletePhotoAsync(userId);

        _logger.LogInformation(
            "User {UserId} deleted the profile photo", userId);

        return NoContent();
    }
}
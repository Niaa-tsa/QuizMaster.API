using QuizMaster.Domain.DTOs.Profile;
using QuizMaster.Domain.Entities;
using QuizMaster.Service.Exceptions;
using QuizMaster.Service.Interfaces;
using QuizMaster.Service.Interfaces.Repositories;

namespace QuizMaster.Service.Services;

public class ProfileService : IProfileService
{
    private const int MaxPhotoBytes = 2 * 1024 * 1024; // 2 MB

    private readonly IUserRepository _userRepository;
    private readonly IProfilePhotoStorage _storage;

    public ProfileService(
        IUserRepository userRepository,
        IProfilePhotoStorage storage)
    {
        _userRepository = userRepository;
        _storage = storage;
    }

    public async Task<ProfileResponseDTO> GetAsync(int userId)
    {
        var user = await GetUserAsync(userId);

        return ToDto(user);
    }

    public async Task<ProfileResponseDTO> UploadPhotoAsync(
        int userId,
        Stream content)
    {
        var user = await GetUserAsync(userId);

        using var buffer = new MemoryStream();
        await content.CopyToAsync(buffer);

        if (buffer.Length == 0)
            throw new BusinessRuleException("The uploaded file is empty.");

        if (buffer.Length > MaxPhotoBytes)
            throw new BusinessRuleException("Photo must be 2 MB or smaller.");

        var bytes = buffer.ToArray();

        var extension = DetectExtension(bytes);

        if (extension is null)
            throw new BusinessRuleException(
                "Only JPEG, PNG and WebP images are allowed.");

        // ფაილის სახელს სერვერი თავად ქმნის, ამიტომ path traversal შეუძლებელია
        var fileName = $"{userId}_{Guid.NewGuid():N}{extension}";

        var oldPhotoUrl = user.ProfilePhotoUrl;

        user.ProfilePhotoUrl = await _storage.SaveAsync(fileName, bytes);
        await _userRepository.SaveChangesAsync();

        if (!string.IsNullOrEmpty(oldPhotoUrl))
            await _storage.DeleteAsync(oldPhotoUrl);

        return ToDto(user);
    }

    public async Task DeletePhotoAsync(int userId)
    {
        var user = await GetUserAsync(userId);

        var oldPhotoUrl = user.ProfilePhotoUrl;

        if (string.IsNullOrEmpty(oldPhotoUrl))
            return;

        user.ProfilePhotoUrl = null;
        await _userRepository.SaveChangesAsync();

        await _storage.DeleteAsync(oldPhotoUrl);
    }

    private async Task<User> GetUserAsync(int userId)
    {
        var user = await _userRepository.GetUserByIdAsync(userId);

        if (user is null)
            throw new NotFoundException("User not found.");

        return user;
    }

    private static string? DetectExtension(byte[] b)
    {
        if (b.Length >= 3 &&
            b[0] == 0xFF && b[1] == 0xD8 && b[2] == 0xFF)
            return ".jpg";

        if (b.Length >= 8 &&
            b[0] == 0x89 && b[1] == 0x50 && b[2] == 0x4E && b[3] == 0x47 &&
            b[4] == 0x0D && b[5] == 0x0A && b[6] == 0x1A && b[7] == 0x0A)
            return ".png";

        if (b.Length >= 12 &&
            b[0] == 0x52 && b[1] == 0x49 && b[2] == 0x46 && b[3] == 0x46 &&
            b[8] == 0x57 && b[9] == 0x45 && b[10] == 0x42 && b[11] == 0x50)
            return ".webp";

        return null;
    }

    private static ProfileResponseDTO ToDto(User user)
    {
        return new ProfileResponseDTO
        {
            Id = user.Id,
            UserName = user.UserName,
            Email = user.Email,
            Role = user.Role.Name,
            ProfilePhotoUrl = user.ProfilePhotoUrl
        };
    }
}
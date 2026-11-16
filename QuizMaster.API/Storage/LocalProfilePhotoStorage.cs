using QuizMaster.Service.Interfaces;

namespace QuizMaster.API.Storage;

/// <summary>
/// ინახავს ფოტოებს wwwroot/uploads/profile-photos საქაღალდეში.
/// </summary>
public class LocalProfilePhotoStorage : IProfilePhotoStorage
{
    private const string RequestPath = "/uploads/profile-photos";

    private readonly string _directory;

    public LocalProfilePhotoStorage(IWebHostEnvironment environment)
    {
        var webRoot = environment.WebRootPath
            ?? Path.Combine(environment.ContentRootPath, "wwwroot");

        _directory = Path.Combine(webRoot, "uploads", "profile-photos");

        Directory.CreateDirectory(_directory);
    }

    public async Task<string> SaveAsync(string fileName, byte[] content)
    {
        var path = Path.Combine(_directory, Path.GetFileName(fileName));

        await File.WriteAllBytesAsync(path, content);

        return $"{RequestPath}/{Path.GetFileName(fileName)}";
    }

    public Task DeleteAsync(string photoUrl)
    {
        var path = Path.Combine(_directory, Path.GetFileName(photoUrl));

        if (File.Exists(path))
            File.Delete(path);

        return Task.CompletedTask;
    }
}
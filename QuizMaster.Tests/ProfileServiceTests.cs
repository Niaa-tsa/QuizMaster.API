using Moq;
using Xunit;
using QuizMaster.Domain.Entities;
using QuizMaster.Service.Exceptions;
using QuizMaster.Service.Interfaces;
using QuizMaster.Service.Interfaces.Repositories;
using QuizMaster.Service.Services;

namespace QuizMaster.Tests;

public class ProfileServiceTests
{
    private readonly Mock<IUserRepository> _userRepoMock;
    private readonly Mock<IProfilePhotoStorage> _storageMock;
    private readonly ProfileService _service;

    public ProfileServiceTests()
    {
        _userRepoMock = new Mock<IUserRepository>();
        _storageMock = new Mock<IProfilePhotoStorage>();
        _service = new ProfileService(_userRepoMock.Object, _storageMock.Object);
    }

    private static User CreateUser()
    {
        return new User
        {
            Id = 1,
            UserName = "student1",
            Email = "student1@test.com",
            Role = new Role { Name = "Student" }
        };
    }

    // PNG-ის სწორი signature + რამდენიმე ბაიტი
    private static byte[] PngBytes()
    {
        return new byte[]
        {
            0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A, 0x00, 0x01, 0x02
        };
    }

    // 1. არა-სურათი (ტექსტი) უნდა უარყოს, თუნდაც სახელი .png იყოს
    [Fact]
    public async Task UploadPhotoAsync_ShouldThrow_WhenFileIsNotAnImage()
    {
        _userRepoMock.Setup(r => r.GetUserByIdAsync(1)).ReturnsAsync(CreateUser());

        using var stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes("hello"));

        await Assert.ThrowsAsync<BusinessRuleException>(
            () => _service.UploadPhotoAsync(1, stream));
    }

    // 2. 2 MB-ზე დიდი ფაილი უნდა უარყოს
    [Fact]
    public async Task UploadPhotoAsync_ShouldThrow_WhenFileIsTooLarge()
    {
        _userRepoMock.Setup(r => r.GetUserByIdAsync(1)).ReturnsAsync(CreateUser());

        using var stream = new MemoryStream(new byte[3 * 1024 * 1024]);

        await Assert.ThrowsAsync<BusinessRuleException>(
            () => _service.UploadPhotoAsync(1, stream));
    }

    // 3. სწორი PNG შეინახება და მომხმარებელს მისამართი ჩაეწერება
    [Fact]
    public async Task UploadPhotoAsync_ShouldSavePhoto_WhenFileIsPng()
    {
        var user = CreateUser();

        _userRepoMock.Setup(r => r.GetUserByIdAsync(1)).ReturnsAsync(user);

        _storageMock
            .Setup(s => s.SaveAsync(It.IsAny<string>(), It.IsAny<byte[]>()))
            .ReturnsAsync("/uploads/profile-photos/1_test.png");

        using var stream = new MemoryStream(PngBytes());

        var result = await _service.UploadPhotoAsync(1, stream);

        Assert.Equal("/uploads/profile-photos/1_test.png", result.ProfilePhotoUrl);

        _storageMock.Verify(
            s => s.SaveAsync(It.Is<string>(n => n.EndsWith(".png")), It.IsAny<byte[]>()),
            Times.Once);
    }

    // 4. არარსებული მომხმარებელი -> NotFoundException
    [Fact]
    public async Task UploadPhotoAsync_ShouldThrowNotFound_WhenUserDoesNotExist()
    {
        _userRepoMock.Setup(r => r.GetUserByIdAsync(It.IsAny<int>()))
            .ReturnsAsync((User?)null);

        using var stream = new MemoryStream(PngBytes());

        await Assert.ThrowsAsync<NotFoundException>(
            () => _service.UploadPhotoAsync(99, stream));
    }
}
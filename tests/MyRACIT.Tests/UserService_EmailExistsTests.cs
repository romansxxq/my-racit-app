using MyRACIT.Services;
using MyRACIT.Tests.Helpers;

namespace MyRACIT.Tests;

public class UserService_EmailExistsTests
{
    [Fact]
    public async Task EmailExistsAsync_ExistingEmail_ReturnsTrue()
    {
        // Arrange
        await using var db = DbFactory.Create(nameof(EmailExistsAsync_ExistingEmail_ReturnsTrue));
        var svc = new UserService(db);
        await svc.CreateAdminAsync("Тест", "exists@test.com", "pass");

        // Act
        var result = await svc.EmailExistsAsync("exists@test.com");

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task EmailExistsAsync_NewEmail_ReturnsFalse()
    {
        // Arrange
        await using var db = DbFactory.Create(nameof(EmailExistsAsync_NewEmail_ReturnsFalse));
        var svc = new UserService(db);

        // Act
        var result = await svc.EmailExistsAsync("new@test.com");

        // Assert
        Assert.False(result);
    }
}

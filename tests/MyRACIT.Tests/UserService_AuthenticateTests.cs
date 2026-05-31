using MyRACIT.Services;
using MyRACIT.Tests.Helpers;

namespace MyRACIT.Tests;

public class UserService_AuthenticateTests
{
    [Fact]
    public async Task AuthenticateAsync_CorrectCredentials_ReturnsUser()
    {
        // Arrange
        await using var db = DbFactory.Create(nameof(AuthenticateAsync_CorrectCredentials_ReturnsUser));
        var svc = new UserService(db);
        await svc.CreateAdminAsync("Адмін", "admin@test.com", "Secret1");

        // Act
        var result = await svc.AuthenticateAsync("admin@test.com", "Secret1");

        // Assert
        Assert.NotNull(result);
        Assert.Equal("admin@test.com", result!.Email);
    }

    [Fact]
    public async Task AuthenticateAsync_WrongPassword_ReturnsNull()
    {
        // Arrange
        await using var db = DbFactory.Create(nameof(AuthenticateAsync_WrongPassword_ReturnsNull));
        var svc = new UserService(db);
        await svc.CreateAdminAsync("Адмін", "admin2@test.com", "Secret1");

        // Act
        var result = await svc.AuthenticateAsync("admin2@test.com", "WrongPass");

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task AuthenticateAsync_UnknownEmail_ReturnsNull()
    {
        // Arrange
        await using var db = DbFactory.Create(nameof(AuthenticateAsync_UnknownEmail_ReturnsNull));
        var svc = new UserService(db);

        // Act
        var result = await svc.AuthenticateAsync("nobody@test.com", "pass");

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task AuthenticateAsync_EmailCaseInsensitive_ReturnsUser()
    {
        // Arrange
        await using var db = DbFactory.Create(nameof(AuthenticateAsync_EmailCaseInsensitive_ReturnsUser));
        var svc = new UserService(db);
        await svc.CreateAdminAsync("Адмін", "Admin@Test.COM", "Pass1");

        // Act
        var result = await svc.AuthenticateAsync("admin@test.com", "Pass1");

        // Assert
        Assert.NotNull(result);
    }
}

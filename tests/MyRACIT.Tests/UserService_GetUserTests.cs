using MyRACIT.Models.Exceptions;
using MyRACIT.Services;
using MyRACIT.Tests.Helpers;

namespace MyRACIT.Tests;

public class UserService_GetUserTests
{
    [Fact]
    public async Task GetUserByIdAsync_ExistingUser_ReturnsUser()
    {
        // Arrange
        await using var db = DbFactory.Create(nameof(GetUserByIdAsync_ExistingUser_ReturnsUser));
        var svc = new UserService(db);
        var created = await svc.CreateAdminAsync("Тест", "getbyid@test.com", "pass");

        // Act
        var result = await svc.GetUserByIdAsync(created.Id);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(created.Id, result!.Id);
    }

    [Fact]
    public async Task GetUserByIdAsync_NonExistentUser_ReturnsNull()
    {
        // Arrange
        await using var db = DbFactory.Create(nameof(GetUserByIdAsync_NonExistentUser_ReturnsNull));
        var svc = new UserService(db);

        // Act
        var result = await svc.GetUserByIdAsync(9999);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetUserByEmailAsync_ExistingEmail_ReturnsUser()
    {
        // Arrange
        await using var db = DbFactory.Create(nameof(GetUserByEmailAsync_ExistingEmail_ReturnsUser));
        var svc = new UserService(db);
        await svc.CreateAdminAsync("Тест", "byemail@test.com", "pass");

        // Act
        var result = await svc.GetUserByEmailAsync("byemail@test.com");

        // Assert
        Assert.NotNull(result);
        Assert.Equal("byemail@test.com", result!.Email);
    }

    [Fact]
    public async Task GetUserByEmailAsync_NonExistentEmail_ReturnsNull()
    {
        // Arrange
        await using var db = DbFactory.Create(nameof(GetUserByEmailAsync_NonExistentEmail_ReturnsNull));
        var svc = new UserService(db);

        // Act
        var result = await svc.GetUserByEmailAsync("nobody@test.com");

        // Assert
        Assert.Null(result);
    }
}

using MyRACIT.Services;
using MyRACIT.Tests.Helpers;

namespace MyRACIT.Tests;

public class UserService_ChangePasswordTests
{
    [Fact]
    public async Task ChangePasswordAsync_CorrectOldPassword_ReturnsTrue()
    {
        // Arrange
        await using var db = DbFactory.Create(nameof(ChangePasswordAsync_CorrectOldPassword_ReturnsTrue));
        var svc = new UserService(db);
        var user = await svc.CreateAdminAsync("Юзер", "chpwd@test.com", "OldPass");

        // Act
        var result = await svc.ChangePasswordAsync(user.Id, "OldPass", "NewPass");

        // Assert
        Assert.True(result);
        Assert.NotNull(await svc.AuthenticateAsync("chpwd@test.com", "NewPass"));
    }

    [Fact]
    public async Task ChangePasswordAsync_WrongOldPassword_ReturnsFalse()
    {
        // Arrange
        await using var db = DbFactory.Create(nameof(ChangePasswordAsync_WrongOldPassword_ReturnsFalse));
        var svc = new UserService(db);
        var user = await svc.CreateAdminAsync("Юзер", "chpwd2@test.com", "OldPass");

        // Act
        var result = await svc.ChangePasswordAsync(user.Id, "WrongOld", "NewPass");

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task ChangePasswordAsync_UnknownUser_ReturnsFalse()
    {
        // Arrange
        await using var db = DbFactory.Create(nameof(ChangePasswordAsync_UnknownUser_ReturnsFalse));
        var svc = new UserService(db);

        // Act
        var result = await svc.ChangePasswordAsync(9999, "old", "new");

        // Assert
        Assert.False(result);
    }
}

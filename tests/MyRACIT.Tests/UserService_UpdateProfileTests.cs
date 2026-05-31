using MyRACIT.Models.Exceptions;
using MyRACIT.Services;
using MyRACIT.Tests.Helpers;

namespace MyRACIT.Tests;

public class UserService_UpdateProfileTests
{
    [Fact]
    public async Task UpdateUserProfileAsync_ValidData_UpdatesNameAndEmail()
    {
        // Arrange
        await using var db = DbFactory.Create(nameof(UpdateUserProfileAsync_ValidData_UpdatesNameAndEmail));
        var svc = new UserService(db);
        var user = await svc.CreateAdminAsync("Старе ім'я", "old@test.com", "pass");

        // Act
        var result = await svc.UpdateUserProfileAsync(user.Id, "Нове ім'я", "new@test.com");

        // Assert
        Assert.True(result);
        var updated = await svc.GetUserByIdAsync(user.Id);
        Assert.Equal("Нове ім'я", updated!.Name);
        Assert.Equal("new@test.com", updated.Email);
    }

    [Fact]
    public async Task UpdateUserProfileAsync_NonExistentUser_ReturnsFalse()
    {
        // Arrange
        await using var db = DbFactory.Create(nameof(UpdateUserProfileAsync_NonExistentUser_ReturnsFalse));
        var svc = new UserService(db);

        // Act
        var result = await svc.UpdateUserProfileAsync(9999, "Ім'я", "email@test.com");

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task UpdateUserProfileAsync_DuplicateEmail_ThrowsInvalidOperationException()
    {
        // Arrange
        await using var db = DbFactory.Create(nameof(UpdateUserProfileAsync_DuplicateEmail_ThrowsInvalidOperationException));
        var svc = new UserService(db);
        await svc.CreateAdminAsync("Перший", "taken@test.com", "pass");
        var second = await svc.CreateAdminAsync("Другий", "second@test.com", "pass");

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => svc.UpdateUserProfileAsync(second.Id, "Другий", "taken@test.com"));
    }

    [Fact]
    public async Task UpdateUserProfileAsync_SameEmail_ReturnsTrue()
    {
        // Arrange
        await using var db = DbFactory.Create(nameof(UpdateUserProfileAsync_SameEmail_ReturnsTrue));
        var svc = new UserService(db);
        var user = await svc.CreateAdminAsync("Тест", "same@test.com", "pass");

        // Act — оновлення з тим самим email (лише ім'я)
        var result = await svc.UpdateUserProfileAsync(user.Id, "Нове Ім'я", "same@test.com");

        // Assert
        Assert.True(result);
    }
}

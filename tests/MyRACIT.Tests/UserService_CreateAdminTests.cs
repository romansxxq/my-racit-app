using MyRACIT.Models.Entities;
using MyRACIT.Models.Exceptions;
using MyRACIT.Services;
using MyRACIT.Tests.Helpers;

namespace MyRACIT.Tests;

public class UserService_CreateAdminTests
{
    [Fact]
    public async Task CreateAdminAsync_ValidData_CreatesUserWithAdminRole()
    {
        // Arrange
        await using var db = DbFactory.Create(nameof(CreateAdminAsync_ValidData_CreatesUserWithAdminRole));
        var svc = new UserService(db);

        // Act
        var user = await svc.CreateAdminAsync("Іван Адмін", "newadmin@test.com", "MyPass1");

        // Assert
        Assert.Equal(UserRole.Admin, user.Role);
        Assert.Equal("Іван Адмін", user.Name);
        Assert.NotEmpty(user.PasswordHash);
        Assert.NotEqual("MyPass1", user.PasswordHash);
    }

    [Fact]
    public async Task CreateAdminAsync_DuplicateEmail_ThrowsInvalidOperationException()
    {
        // Arrange
        await using var db = DbFactory.Create(nameof(CreateAdminAsync_DuplicateEmail_ThrowsInvalidOperationException));
        var svc = new UserService(db);
        await svc.CreateAdminAsync("Перший", "dup@test.com", "pass");

        // Act & Assert
        await Assert.ThrowsAsync<UserAlreadyExistsException>(
            () => svc.CreateAdminAsync("Другий", "dup@test.com", "pass2"));
    }
}

using MyRACIT.Models.Entities;
using MyRACIT.Models.Exceptions;
using MyRACIT.Services;
using MyRACIT.Tests.Helpers;

namespace MyRACIT.Tests;

public class UserService_CreateTeacherTests
{
    private static async Task<(MyRACIT.Data.MyRacitDbContext db, Department dept)> SetupDbWithDepartment(string name)
    {
        var db = DbFactory.Create(name);
        var dept = new Department { Name = "Кафедра ІТ" };
        db.Departments.Add(dept);
        await db.SaveChangesAsync();
        return (db, dept);
    }

    [Fact]
    public async Task CreateTeacherAsync_ValidData_CreatesTeacherProfile()
    {
        // Arrange
        var (db, dept) = await SetupDbWithDepartment(nameof(CreateTeacherAsync_ValidData_CreatesTeacherProfile));
        await using var _ = db;
        var svc = new UserService(db);

        // Act
        var profile = await svc.CreateTeacherAsync("Іванов Іван", "ivan@test.com", "pass", dept.Id);

        // Assert
        Assert.NotNull(profile);
        Assert.Equal(dept.Id, profile!.DepartmentId);
        Assert.Equal(UserRole.Teacher, profile.User!.Role);
        Assert.Equal("Іванов Іван", profile.User.Name);
    }

    [Fact]
    public async Task CreateTeacherAsync_NonExistentDepartment_ThrowsDepartmentNotFoundException()
    {
        // Arrange
        await using var db = DbFactory.Create(nameof(CreateTeacherAsync_NonExistentDepartment_ThrowsDepartmentNotFoundException));
        var svc = new UserService(db);

        // Act & Assert
        await Assert.ThrowsAsync<DepartmentNotFoundException>(
            () => svc.CreateTeacherAsync("Викладач", "t@test.com", "pass", 9999));
    }

    [Fact]
    public async Task CreateTeacherAsync_DuplicateEmail_ThrowsUserAlreadyExistsException()
    {
        // Arrange
        var (db, dept) = await SetupDbWithDepartment(nameof(CreateTeacherAsync_DuplicateEmail_ThrowsUserAlreadyExistsException));
        await using var _ = db;
        var svc = new UserService(db);
        await svc.CreateTeacherAsync("Перший", "dup.teacher@test.com", "pass", dept.Id);

        // Act & Assert
        await Assert.ThrowsAsync<UserAlreadyExistsException>(
            () => svc.CreateTeacherAsync("Другий", "dup.teacher@test.com", "pass2", dept.Id));
    }
}

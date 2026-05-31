using MyRACIT.Data;
using MyRACIT.Models.Entities;
using MyRACIT.Models.Exceptions;
using MyRACIT.Services;
using MyRACIT.Tests.Helpers;

namespace MyRACIT.Tests;

public class UserService_CreateStudentTests
{
    private static async Task<(MyRacitDbContext db, Group group)> SetupDbWithGroup(string name)
    {
        var db = DbFactory.Create(name);
        var specialty = new Specialty { Name = "ІПЗ", Code = "121" };
        db.Specialties.Add(specialty);
        await db.SaveChangesAsync();

        var group = new Group { Name = "ІПЗ-21", SpecialtyId = specialty.Id, StudyYear = 2021 };
        db.Groups.Add(group);
        await db.SaveChangesAsync();

        return (db, group);
    }

    [Fact]
    public async Task CreateStudentAsync_ValidData_CreatesStudentProfile()
    {
        // Arrange
        var (db, group) = await SetupDbWithGroup(nameof(CreateStudentAsync_ValidData_CreatesStudentProfile));
        await using var _ = db;
        var svc = new UserService(db);

        // Act
        var profile = await svc.CreateStudentAsync("Тарас Шевченко", "taras@test.com", "pass", group.Id);

        // Assert
        Assert.NotNull(profile);
        Assert.Equal(group.Id, profile.GroupId);
        Assert.Equal(UserRole.Student, profile.User.Role);
    }

    [Fact]
    public async Task CreateStudentAsync_NonExistentGroup_ThrowsArgumentException()
    {
        // Arrange
        await using var db = DbFactory.Create(nameof(CreateStudentAsync_NonExistentGroup_ThrowsArgumentException));
        var svc = new UserService(db);

        // Act & Assert
        await Assert.ThrowsAsync<GroupNotFoundException>(
            () => svc.CreateStudentAsync("Студент", "s@test.com", "pass", 9999));
    }

    [Fact]
    public async Task CreateStudentAsync_DuplicateEmail_ThrowsInvalidOperationException()
    {
        // Arrange
        var (db, group) = await SetupDbWithGroup(nameof(CreateStudentAsync_DuplicateEmail_ThrowsInvalidOperationException));
        await using var _ = db;
        var svc = new UserService(db);
        await svc.CreateStudentAsync("Перший", "dup.student@test.com", "pass", group.Id);

        // Act & Assert
        await Assert.ThrowsAsync<UserAlreadyExistsException>(
            () => svc.CreateStudentAsync("Другий", "dup.student@test.com", "pass2", group.Id));
    }
}

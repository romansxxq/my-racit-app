using Microsoft.AspNetCore.Mvc;
using Moq;
using MyRACIT.Controllers;
using MyRACIT.Models.Entities;
using MyRACIT.Services.Interfaces;
using MyRACIT.Tests.Helpers;

namespace MyRACIT.Tests;

public class TeacherControllerTests
{
    [Fact]
    public async Task Index_TeacherNotFound_ReturnsNotFound()
    {
        var db = DbFactory.Create(nameof(Index_TeacherNotFound_ReturnsNotFound));
        var ctrl = new TeacherController(db, new Mock<IFileStorageService>().Object);
        ClaimsHelper.SetUser(ctrl, 999, "Teacher");

        var result = await ctrl.Index();

        Assert.IsType<NotFoundObjectResult>(result);
    }

    [Fact]
    public async Task Index_ValidTeacher_ReturnsView()
    {
        var db = DbFactory.Create(nameof(Index_ValidTeacher_ReturnsView));

        var dept = new Department { Name = "ІТ" };
        db.Departments.Add(dept);

        var user = new User { Name = "Марія Іваненко", Email = "maria@test.com", PasswordHash = "x", Role = UserRole.Teacher };
        db.Users.Add(user);
        await db.SaveChangesAsync();

        db.TeacherProfiles.Add(new TeacherProfile { UserId = user.Id, DepartmentId = dept.Id });
        await db.SaveChangesAsync();

        var ctrl = new TeacherController(db, new Mock<IFileStorageService>().Object);
        ClaimsHelper.SetUser(ctrl, user.Id, "Teacher");

        var result = await ctrl.Index();

        Assert.IsType<ViewResult>(result);
    }
}

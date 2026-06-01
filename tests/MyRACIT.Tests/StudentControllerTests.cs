using Microsoft.AspNetCore.Mvc;
using MyRACIT.Controllers;
using MyRACIT.Models.Entities;
using MyRACIT.Tests.Helpers;

namespace MyRACIT.Tests;

public class StudentControllerTests
{
    [Fact]
    public async Task Index_StudentNotFound_ReturnsNotFound()
    {
        var db = DbFactory.Create(nameof(Index_StudentNotFound_ReturnsNotFound));
        var ctrl = new StudentController(db);
        ClaimsHelper.SetUser(ctrl, 999, "Student");

        var result = await ctrl.Index();

        Assert.IsType<NotFoundObjectResult>(result);
    }

    [Fact]
    public async Task Index_ValidStudent_ReturnsView()
    {
        var db = DbFactory.Create(nameof(Index_ValidStudent_ReturnsView));

        var spec = new Specialty { Code = "122", Name = "Комп'ютерні науки" };
        db.Specialties.Add(spec);
        await db.SaveChangesAsync();

        var group = new Group { Name = "ПЗ-11", StudyYear = 1, SpecialtyId = spec.Id };
        db.Groups.Add(group);

        var user = new User { Name = "Тарас Шевченко", Email = "taras@test.com", PasswordHash = "x", Role = UserRole.Student };
        db.Users.Add(user);
        await db.SaveChangesAsync();

        db.StudentProfiles.Add(new StudentProfile { UserId = user.Id, GroupId = group.Id });
        await db.SaveChangesAsync();

        var ctrl = new StudentController(db);
        ClaimsHelper.SetUser(ctrl, user.Id, "Student");

        var result = await ctrl.Index();

        Assert.IsType<ViewResult>(result);
    }

    [Fact]
    public async Task MyCourses_StudentNotFound_ReturnsNotFound()
    {
        var db = DbFactory.Create(nameof(MyCourses_StudentNotFound_ReturnsNotFound));
        var ctrl = new StudentController(db);
        ClaimsHelper.SetUser(ctrl, 999, "Student");

        var result = await ctrl.MyCourses();

        Assert.IsType<NotFoundObjectResult>(result);
    }
}

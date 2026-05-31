using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.EntityFrameworkCore;
using Moq;
using MyRACIT.Controllers;
using MyRACIT.Models.Entities;
using MyRACIT.Services.Interfaces;
using MyRACIT.Tests.Helpers;

namespace MyRACIT.Tests;

public class AdminControllerTests
{
    [Fact]
    public async Task Index_ReturnsViewWithCorrectStats()
    {
        var db = DbFactory.Create(nameof(Index_ReturnsViewWithCorrectStats));
        db.Departments.Add(new Department { Name = "Кафедра ІТ" });
        await db.SaveChangesAsync();

        var ctrl = new AdminController(new Mock<IUserService>().Object, db);

        var result = await ctrl.Index();

        Assert.IsType<ViewResult>(result);
        Assert.Equal(1, ctrl.ViewBag.TotalDepartments);
        Assert.Equal(0, ctrl.ViewBag.TotalStudents);
    }

    [Fact]
    public async Task Departments_ReturnsViewWithAllDepartments()
    {
        var db = DbFactory.Create(nameof(Departments_ReturnsViewWithAllDepartments));
        db.Departments.AddRange(
            new Department { Name = "Кафедра 1" },
            new Department { Name = "Кафедра 2" });
        await db.SaveChangesAsync();

        var ctrl = new AdminController(new Mock<IUserService>().Object, db);

        var result = await ctrl.Departments();

        var view = Assert.IsType<ViewResult>(result);
        var model = Assert.IsAssignableFrom<IEnumerable<Department>>(view.Model);
        Assert.Equal(2, model.Count());
    }

    [Fact]
    public async Task CreateDepartment_InvalidModelState_ReturnsView()
    {
        var db = DbFactory.Create(nameof(CreateDepartment_InvalidModelState_ReturnsView));
        var ctrl = new AdminController(new Mock<IUserService>().Object, db);
        ctrl.ModelState.AddModelError("Name", "Required");

        var result = await ctrl.CreateDepartment(new Department { Name = "" });

        Assert.IsType<ViewResult>(result);
    }

    [Fact]
    public async Task CreateDepartment_ValidData_RedirectsAndSaves()
    {
        var db = DbFactory.Create(nameof(CreateDepartment_ValidData_RedirectsAndSaves));
        var ctrl = new AdminController(new Mock<IUserService>().Object, db);
        ctrl.TempData = new Mock<ITempDataDictionary>().Object;

        var result = await ctrl.CreateDepartment(new Department { Name = "Нова кафедра" });

        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Departments", redirect.ActionName);
        Assert.Equal(1, await db.Departments.CountAsync());
    }
}

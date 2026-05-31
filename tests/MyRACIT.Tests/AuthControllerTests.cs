using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Moq;
using MyRACIT.Controllers;
using MyRACIT.Models.DTOs;
using MyRACIT.Models.Entities;
using MyRACIT.Services.Interfaces;

namespace MyRACIT.Tests;

public class AuthControllerTests
{
    private static AuthController BuildController(
        Mock<IUserService>? userSvcMock = null,
        bool isAuthenticated = false)
    {
        userSvcMock ??= new Mock<IUserService>();

        var authServiceMock = new Mock<IAuthenticationService>();
        authServiceMock
            .Setup(a => a.SignInAsync(It.IsAny<HttpContext>(), It.IsAny<string>(), It.IsAny<System.Security.Claims.ClaimsPrincipal>(), It.IsAny<AuthenticationProperties>()))
            .Returns(Task.CompletedTask);
        authServiceMock
            .Setup(a => a.SignOutAsync(It.IsAny<HttpContext>(), It.IsAny<string>(), It.IsAny<AuthenticationProperties>()))
            .Returns(Task.CompletedTask);

        var serviceProvider = new Mock<IServiceProvider>();
        serviceProvider
            .Setup(sp => sp.GetService(typeof(IAuthenticationService)))
            .Returns(authServiceMock.Object);

        var httpContext = new DefaultHttpContext { RequestServices = serviceProvider.Object };

        if (isAuthenticated)
        {
            var identity = new System.Security.Claims.ClaimsIdentity(
                new[] { new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.Name, "Test") },
                "TestAuth");
            httpContext.User = new System.Security.Claims.ClaimsPrincipal(identity);
        }

        var ctrl = new AuthController(userSvcMock.Object)
        {
            ControllerContext = new ControllerContext { HttpContext = httpContext },
            TempData = new Mock<ITempDataDictionary>().Object
        };

        var urlHelper = new Mock<IUrlHelper>();
        urlHelper.Setup(u => u.IsLocalUrl(It.IsAny<string?>())).Returns(false);
        ctrl.Url = urlHelper.Object;

        return ctrl;
    }

    // ---- Login GET ----

    [Fact]
    public void Login_Get_NotAuthenticated_ReturnsView()
    {
        var ctrl = BuildController();

        var result = ctrl.Login(null);

        Assert.IsType<ViewResult>(result);
    }

    [Fact]
    public void Login_Get_AlreadyAuthenticated_Redirects()
    {
        var ctrl = BuildController(isAuthenticated: true);

        var result = ctrl.Login(null);

        Assert.IsType<RedirectToActionResult>(result);
    }

    // ---- Login POST ----

    [Fact]
    public async Task Login_Post_InvalidModelState_ReturnsView()
    {
        var ctrl = BuildController();
        ctrl.ModelState.AddModelError("Email", "Required");

        var result = await ctrl.Login(new LoginDto(), null);

        Assert.IsType<ViewResult>(result);
    }

    [Fact]
    public async Task Login_Post_WrongCredentials_ReturnsViewWithError()
    {
        var svc = new Mock<IUserService>();
        svc.Setup(s => s.AuthenticateAsync(It.IsAny<string>(), It.IsAny<string>()))
           .ReturnsAsync((User?)null);

        var ctrl = BuildController(svc);

        var result = await ctrl.Login(new LoginDto { Email = "x@x.com", Password = "wrong" }, null);

        var view = Assert.IsType<ViewResult>(result);
        Assert.False(ctrl.ModelState.IsValid);
    }

    [Fact]
    public async Task Login_Post_ValidCredentials_Admin_RedirectsToAdmin()
    {
        var svc = new Mock<IUserService>();
        svc.Setup(s => s.AuthenticateAsync(It.IsAny<string>(), It.IsAny<string>()))
           .ReturnsAsync(new User { Id = 1, Name = "Admin", Email = "a@a.com", Role = UserRole.Admin, PasswordHash = "x" });

        var ctrl = BuildController(svc);

        var result = await ctrl.Login(new LoginDto { Email = "a@a.com", Password = "pass" }, null);

        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Index", redirect.ActionName);
        Assert.Equal("Admin", redirect.ControllerName);
    }

    [Fact]
    public async Task Login_Post_ValidCredentials_Student_RedirectsToStudent()
    {
        var svc = new Mock<IUserService>();
        svc.Setup(s => s.AuthenticateAsync(It.IsAny<string>(), It.IsAny<string>()))
           .ReturnsAsync(new User { Id = 2, Name = "Student", Email = "s@s.com", Role = UserRole.Student, PasswordHash = "x" });

        var ctrl = BuildController(svc);

        var result = await ctrl.Login(new LoginDto { Email = "s@s.com", Password = "pass" }, null);

        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Index", redirect.ActionName);
        Assert.Equal("Student", redirect.ControllerName);
    }

    // ---- Logout ----

    [Fact]
    public async Task Logout_RedirectsToLogin()
    {
        var ctrl = BuildController();

        var result = await ctrl.Logout();

        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Login", redirect.ActionName);
    }

    // ---- AccessDenied ----

    [Fact]
    public void AccessDenied_ReturnsView()
    {
        var ctrl = BuildController();

        var result = ctrl.AccessDenied(null);

        Assert.IsType<ViewResult>(result);
    }
}

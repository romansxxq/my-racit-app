using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using MyRACIT.Models;

namespace MyRACIT.Controllers;

public class HomeController : Controller
{
    public IActionResult Index()
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            var role = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;
            return role switch
            {
                "Admin" => RedirectToAction("Index", "Admin"),
                "Teacher" => RedirectToAction("Index", "Teacher"),
                "Student" => RedirectToAction("Index", "Student"),
                _ => RedirectToAction("Login", "Auth")
            };
        }
        return RedirectToAction("Login", "Auth");
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}

// =====================================================
// ПРИКЛАД 1: Реєстрація сервісу в Program.cs
// =====================================================

/*
using MyRACIT.Services;
using MyRACIT.Services.Interfaces;

var builder = WebApplication.CreateBuilder(args);

// Додати DbContext (якщо ще не додано)
builder.Services.AddDbContext<MyRacitDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Реєструємо UserService
builder.Services.AddScoped<IUserService, UserService>();

var app = builder.Build();
*/


// =====================================================
// ПРИКЛАД 2: Використання в AdminController
// =====================================================

/*
using Microsoft.AspNetCore.Mvc;
using MyRACIT.Services.Interfaces;
using MyRACIT.Models.DTOs;

public class AdminController : Controller
{
    private readonly IUserService _userService;
    
    public AdminController(IUserService userService)
    {
        _userService = userService;
    }
    
    // GET: Admin/CreateStudent
    public IActionResult CreateStudent()
    {
        // Завантажити список груп для dropdown
        return View();
    }
    
    // POST: Admin/CreateStudent
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateStudent(CreateStudentDto dto)
    {
        if (!ModelState.IsValid)
        {
            return View(dto);
        }
        
        try
        {
            var studentProfile = await _userService.CreateStudentAsync(
                dto.Name, 
                dto.Email, 
                dto.Password, 
                dto.GroupId
            );
            
            TempData["Success"] = $"Студента {dto.Name} успішно створено!";
            return RedirectToAction("Students");
        }
        catch (InvalidOperationException ex)
        {
            ModelState.AddModelError("", ex.Message);
            return View(dto);
        }
    }
    
    // POST: Admin/CreateTeacher
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateTeacher(CreateTeacherDto dto)
    {
        if (!ModelState.IsValid)
        {
            return View(dto);
        }
        
        try
        {
            var teacherProfile = await _userService.CreateTeacherAsync(
                dto.Name, 
                dto.Email, 
                dto.Password, 
                dto.DepartmentId
            );
            
            TempData["Success"] = $"Викладача {dto.Name} успішно створено!";
            return RedirectToAction("Teachers");
        }
        catch (Exception ex)
        {
            ModelState.AddModelError("", ex.Message);
            return View(dto);
        }
    }
}
*/


// =====================================================
// ПРИКЛАД 3: Використання в AuthController
// =====================================================

/*
using Microsoft.AspNetCore.Mvc;
using MyRACIT.Services.Interfaces;
using MyRACIT.Models.DTOs;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Claims;

public class AuthController : Controller
{
    private readonly IUserService _userService;
    
    public AuthController(IUserService userService)
    {
        _userService = userService;
    }
    
    // GET: Auth/Login
    public IActionResult Login()
    {
        return View();
    }
    
    // POST: Auth/Login
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginDto dto)
    {
        if (!ModelState.IsValid)
        {
            return View(dto);
        }
        
        // Аутентифікація через сервіс
        var user = await _userService.AuthenticateAsync(dto.Email, dto.Password);
        
        if (user == null)
        {
            ModelState.AddModelError("", "Невірний email або пароль");
            return View(dto);
        }
        
        // Створюємо claims для Cookie Authentication
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.Name),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Role, user.Role.ToString())
        };
        
        var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var authProperties = new AuthenticationProperties
        {
            IsPersistent = true, // Remember me
            ExpiresUtc = DateTimeOffset.UtcNow.AddDays(7)
        };
        
        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            new ClaimsPrincipal(claimsIdentity),
            authProperties
        );
        
        // Перенаправлення залежно від ролі
        return user.Role switch
        {
            UserRole.Admin => RedirectToAction("Index", "Admin"),
            UserRole.Teacher => RedirectToAction("MyCourses", "Teacher"),
            UserRole.Student => RedirectToAction("MyCourses", "Student"),
            _ => RedirectToAction("Index", "Home")
        };
    }
    
    // POST: Auth/Logout
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToAction("Login");
    }
}
*/


// =====================================================
// ПРИКЛАД 4: Зміна пароля в профілі
// =====================================================

/*
public class StudentController : Controller
{
    private readonly IUserService _userService;
    
    public StudentController(IUserService userService)
    {
        _userService = userService;
    }
    
    // GET: Student/ChangePassword
    public IActionResult ChangePassword()
    {
        return View();
    }
    
    // POST: Student/ChangePassword
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ChangePassword(ChangePasswordDto dto)
    {
        if (!ModelState.IsValid)
        {
            return View(dto);
        }
        
        // Отримуємо ID поточного користувача з Claims
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userIdClaim == null)
        {
            return RedirectToAction("Login", "Auth");
        }
        
        int userId = int.Parse(userIdClaim);
        
        bool success = await _userService.ChangePasswordAsync(
            userId, 
            dto.OldPassword, 
            dto.NewPassword
        );
        
        if (success)
        {
            TempData["Success"] = "Пароль успішно змінено!";
            return RedirectToAction("Profile");
        }
        else
        {
            ModelState.AddModelError("", "Невірний старий пароль");
            return View(dto);
        }
    }
}
*/


// =====================================================
// ПРИКЛАД 5: Отримання профілю студента
// =====================================================

/*
public class StudentController : Controller
{
    private readonly IUserService _userService;
    
    // GET: Student/Profile
    public async Task<IActionResult> Profile()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userIdClaim == null)
        {
            return RedirectToAction("Login", "Auth");
        }
        
        int userId = int.Parse(userIdClaim);
        
        var studentProfile = await _userService.GetStudentProfileAsync(userId);
        
        if (studentProfile == null)
        {
            return NotFound();
        }
        
        return View(studentProfile);
    }
}
*/


// =====================================================
// ПРИКЛАД 6: View для створення студента
// =====================================================

/*
@model CreateStudentDto

<h2>Створити студента</h2>

<form asp-action="CreateStudent" method="post">
    <div class="form-group">
        <label asp-for="Name"></label>
        <input asp-for="Name" class="form-control" />
        <span asp-validation-for="Name" class="text-danger"></span>
    </div>
    
    <div class="form-group">
        <label asp-for="Email"></label>
        <input asp-for="Email" class="form-control" type="email" />
        <span asp-validation-for="Email" class="text-danger"></span>
    </div>
    
    <div class="form-group">
        <label asp-for="Password"></label>
        <input asp-for="Password" class="form-control" type="password" />
        <span asp-validation-for="Password" class="text-danger"></span>
    </div>
    
    <div class="form-group">
        <label asp-for="GroupId">Група</label>
        <select asp-for="GroupId" class="form-control">
            <option value="">-- Виберіть групу --</option>
            <!-- Завантажити з ViewBag або ViewData -->
        </select>
        <span asp-validation-for="GroupId" class="text-danger"></span>
    </div>
    
    <button type="submit" class="btn btn-primary">Створити</button>
</form>

@section Scripts {
    @{await Html.RenderPartialAsync("_ValidationScriptsPartial");}
}
*/

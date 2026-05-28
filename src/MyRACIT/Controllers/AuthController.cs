using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyRACIT.Models.DTOs;
using MyRACIT.Services.Interfaces;
using System.Security.Claims;

namespace MyRACIT.Controllers
{
    /// <summary>
    /// Контролер для аутентифікації (UC18)
    /// </summary>
    public class AuthController : Controller
    {
        private readonly IUserService _userService;
        
        public AuthController(IUserService userService)
        {
            _userService = userService;
        }
        
        // ===== UC18: LOGIN =====
        
        // GET: Auth/Login
        [AllowAnonymous]
        public IActionResult Login(string? returnUrl = null)
        {
            // Якщо вже авторизований - редірект на головну
            if (User.Identity?.IsAuthenticated == true)
            {
                return RedirectToHome();
            }
            
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }
        
        // POST: Auth/Login
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginDto dto, string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            
            if (!ModelState.IsValid)
            {
                return View(dto);
            }
            
            try
            {
                // Аутентифікація через UserService
                var user = await _userService.AuthenticateAsync(dto.Email, dto.Password);
                
                if (user == null)
                {
                    ModelState.AddModelError(string.Empty, "Невірний email або пароль");
                    return View(dto);
                }
                
                // Створюємо Claims для Cookie
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                    new Claim(ClaimTypes.Name, user.Name),
                    new Claim(ClaimTypes.Email, user.Email),
                    new Claim(ClaimTypes.Role, user.Role.ToString())
                };
                
                var claimsIdentity = new ClaimsIdentity(
                    claims, 
                    CookieAuthenticationDefaults.AuthenticationScheme
                );
                
                var authProperties = new AuthenticationProperties
                {
                    IsPersistent = dto.RememberMe,
                    ExpiresUtc = dto.RememberMe 
                        ? DateTimeOffset.UtcNow.AddDays(30) 
                        : DateTimeOffset.UtcNow.AddHours(8)
                };
                
                // Sign In
                await HttpContext.SignInAsync(
                    CookieAuthenticationDefaults.AuthenticationScheme,
                    new ClaimsPrincipal(claimsIdentity),
                    authProperties
                );
                
                TempData["Success"] = $"Вітаємо, {user.Name}!";
                
                // Редірект залежно від ролі
                return RedirectAfterLogin(user.Role.ToString(), returnUrl);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, $"Помилка входу: {ex.Message}");
                return View(dto);
            }
        }
        
        // ===== UC18: LOGOUT =====
        
        // POST: Auth/Logout
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            
            TempData["Info"] = "Ви успішно вийшли з системи";
            return RedirectToAction(nameof(Login));
        }
        
        // ===== ACCESS DENIED =====
        
        // GET: Auth/AccessDenied
        [AllowAnonymous]
        public IActionResult AccessDenied(string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }
        
        // ===== HELPER METHODS =====
        
        /// <summary>
        /// Редірект на головну сторінку залежно від ролі користувача
        /// </summary>
        private IActionResult RedirectToHome()
        {
            if (User.IsInRole("Admin"))
            {
                return RedirectToAction("Index", "Admin");
            }
            else if (User.IsInRole("Teacher"))
            {
                return RedirectToAction("Index", "Teacher");
            }
            else if (User.IsInRole("Student"))
            {
                return RedirectToAction("Index", "Student");
            }
            
            return RedirectToAction("Index", "Home");
        }
        
        /// <summary>
        /// Редірект після успішного логіну
        /// </summary>
        private IActionResult RedirectAfterLogin(string role, string? returnUrl)
        {
            // Якщо є returnUrl і він локальний - редірект туди
            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            
            // Інакше редірект залежно від ролі
            switch (role)
            {
                case "Admin":
                    return RedirectToAction("Index", "Admin");
                
                case "Teacher":
                    return RedirectToAction("Index", "Teacher");
                
                case "Student":
                    return RedirectToAction("Index", "Student");
                
                default:
                    return RedirectToAction("Index", "Home");
            }
        }
    }
}

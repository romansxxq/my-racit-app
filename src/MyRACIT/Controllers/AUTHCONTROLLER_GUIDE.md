# AuthController - Довідник методів

## 📊 Загальна інформація

**Роль доступу:** Публічний (`[AllowAnonymous]` на Login, інші методи захищені)  
**Use Cases:** UC18 (Login, Logout)  
**Залежності:** `IUserService`  
**Схема аутентифікації:** Cookie Authentication

---

## 🔐 UC18: АУТЕНТИФІКАЦІЯ

### `Login()` - GET
- **GET** `/Auth/Login?returnUrl=/admin`
- Форма входу в систему
- Підтримує returnUrl для редіректу після логіну

**Логіка:**
```csharp
// Якщо вже авторизований - редірект на головну
if (User.Identity?.IsAuthenticated == true)
{
    return RedirectToHome();  // залежно від ролі
}
```

**ViewData:**
- `ReturnUrl` - URL для редіректу після входу

---

### `Login(LoginDto)` - POST
- **POST** `/Auth/Login`
- Обробка форми входу
- Перевірка облікових даних через UserService
- Створення Cookie з Claims

**Процес аутентифікації:**

#### 1. Перевірка облікових даних
```csharp
var user = await _userService.AuthenticateAsync(dto.Email, dto.Password);

if (user == null)
{
    ModelState.AddModelError(string.Empty, "Невірний email або пароль");
    return View(dto);
}
```

#### 2. Створення Claims
```csharp
var claims = new List<Claim>
{
    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
    new Claim(ClaimTypes.Name, user.Name),
    new Claim(ClaimTypes.Email, user.Email),
    new Claim(ClaimTypes.Role, user.Role.ToString())
};
```

**Claims:**
- `NameIdentifier` - UserId (для GetCurrentStudentProfileAsync та інших)
- `Name` - Ім'я користувача
- `Email` - Email
- `Role` - Admin/Teacher/Student (для `[Authorize(Roles = "...")]`)

#### 3. Налаштування Cookie
```csharp
var authProperties = new AuthenticationProperties
{
    IsPersistent = dto.RememberMe,
    ExpiresUtc = dto.RememberMe 
        ? DateTimeOffset.UtcNow.AddDays(30)   // "Запам'ятати мене"
        : DateTimeOffset.UtcNow.AddHours(8)   // Звичайна сесія
};
```

**Параметри:**
- `IsPersistent` - зберігати Cookie після закриття браузера
- `ExpiresUtc` - час життя Cookie

#### 4. Sign In
```csharp
await HttpContext.SignInAsync(
    CookieAuthenticationDefaults.AuthenticationScheme,
    new ClaimsPrincipal(claimsIdentity),
    authProperties
);
```

#### 5. Редірект залежно від ролі
```csharp
return RedirectAfterLogin(user.Role.ToString(), returnUrl);

// Admin → /Admin/Index
// Teacher → /Teacher/Index
// Student → /Student/Index
```

---

### `Logout()` - POST
- **POST** `/Auth/Logout`
- Вихід з системи
- Видаляє Cookie аутентифікації

**Логіка:**
```csharp
await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

TempData["Info"] = "Ви успішно вийшли з системи";
return RedirectToAction(nameof(Login));
```

**Важливо:** Метод має бути POST для захисту від CSRF!

---

### `AccessDenied()`
- **GET** `/Auth/AccessDenied?returnUrl=/admin`
- Сторінка "Доступ заборонено"
- Показується, коли користувач намагається увійти на сторінку без прав

**Коли показується:**
- Student намагається зайти на `/Admin`
- Teacher намагається зайти на `/Student`
- Неавторизований користувач на захищену сторінку

---

## 🛠️ HELPER METHODS

### `RedirectToHome()`
**Приватний метод**

Редірект на головну сторінку залежно від поточної ролі:

```csharp
if (User.IsInRole("Admin"))
    return RedirectToAction("Index", "Admin");
else if (User.IsInRole("Teacher"))
    return RedirectToAction("Index", "Teacher");
else if (User.IsInRole("Student"))
    return RedirectToAction("Index", "Student");
    
return RedirectToAction("Index", "Home");
```

**Використання:** Коли авторизований користувач заходить на `/Auth/Login`

---

### `RedirectAfterLogin(role, returnUrl)`
**Приватний метод**

Редірект після успішного логіну з пріоритетом returnUrl:

```csharp
// 1. Пріоритет: returnUrl (якщо є і локальний)
if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
{
    return Redirect(returnUrl);
}

// 2. Інакше: залежно від ролі
switch (role)
{
    case "Admin": return RedirectToAction("Index", "Admin");
    case "Teacher": return RedirectToAction("Index", "Teacher");
    case "Student": return RedirectToAction("Index", "Student");
    default: return RedirectToAction("Index", "Home");
}
```

**Безпека:** `Url.IsLocalUrl(returnUrl)` захищає від Open Redirect атак!

---

## 🔐 Схема Cookie Authentication

### Налаштування в Program.cs

```csharp
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Auth/Login";
        options.LogoutPath = "/Auth/Logout";
        options.AccessDeniedPath = "/Auth/AccessDenied";
        options.ExpireTimeSpan = TimeSpan.FromHours(8);
        options.SlidingExpiration = true;
    });

builder.Services.AddAuthorization();

// В middleware pipeline:
app.UseAuthentication();
app.UseAuthorization();
```

**Параметри:**
- `LoginPath` - куди редірект при неавторизованому доступі
- `AccessDeniedPath` - куди редірект при недостатніх правах
- `ExpireTimeSpan` - час життя Cookie за замовчуванням
- `SlidingExpiration` - продовжувати час життя при активності

---

## 📊 Потік аутентифікації

```
1. Користувач → /Auth/Login
   └─> Показує форму

2. POST /Auth/Login (email, password, rememberMe)
   └─> UserService.AuthenticateAsync()
       ├─> Перевірка email
       ├─> Перевірка password hash
       └─> Повертає User або null

3. Створення Claims (UserId, Name, Email, Role)

4. HttpContext.SignInAsync()
   └─> Створює Cookie з зашифрованими Claims

5. Редірект залежно від ролі:
   ├─> Admin → /Admin/Index
   ├─> Teacher → /Teacher/Index
   └─> Student → /Student/Index

6. Наступні запити:
   └─> Cookie автоматично додається до запитів
       └─> User.Identity.IsAuthenticated = true
       └─> User.IsInRole("Admin") = true/false
       └─> User.FindFirstValue(ClaimTypes.NameIdentifier) = "123"
```

---

## 🔓 Logout потік

```
1. POST /Auth/Logout
   └─> HttpContext.SignOutAsync()
       └─> Видаляє Cookie

2. Редірект → /Auth/Login
   └─> User.Identity.IsAuthenticated = false
```

---

## 🛡️ Безпека

### 1. CSRF Protection
```csharp
[ValidateAntiForgeryToken]  // На всіх POST методах
```

### 2. Open Redirect Protection
```csharp
if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
{
    return Redirect(returnUrl);
}
```

**Захищає від:** `?returnUrl=http://evil.com`

### 3. Password Hashing
Здійснюється в `UserService.AuthenticateAsync()` через SHA256 (або BCrypt)

### 4. Cookie Security
- HttpOnly = true (не доступно через JavaScript)
- Secure = true (тільки HTTPS у production)
- SameSite = Lax (захист від CSRF)

---

## 📝 LoginDto

```csharp
public class LoginDto
{
    [Required(ErrorMessage = "Email обов'язковий")]
    [EmailAddress(ErrorMessage = "Невірний формат email")]
    public string Email { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "Пароль обов'язковий")]
    [DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;
    
    [Display(Name = "Запам'ятати мене")]
    public bool RememberMe { get; set; }
}
```

---

## 📂 Необхідні Views

```
Views/Auth/
├── Login.cshtml              (Форма входу)
└── AccessDenied.cshtml       (Сторінка "Доступ заборонено")
```

### Login.cshtml приклад:
```cshtml
@model LoginDto

<h2>Вхід в систему</h2>

<form asp-action="Login" method="post">
    <input type="hidden" name="returnUrl" value="@ViewData["ReturnUrl"]" />
    
    <div asp-validation-summary="ModelOnly" class="text-danger"></div>
    
    <div class="form-group">
        <label asp-for="Email"></label>
        <input asp-for="Email" class="form-control" />
        <span asp-validation-for="Email" class="text-danger"></span>
    </div>
    
    <div class="form-group">
        <label asp-for="Password"></label>
        <input asp-for="Password" class="form-control" type="password" />
        <span asp-validation-for="Password" class="text-danger"></span>
    </div>
    
    <div class="form-check">
        <input asp-for="RememberMe" class="form-check-input" />
        <label asp-for="RememberMe" class="form-check-label"></label>
    </div>
    
    <button type="submit" class="btn btn-primary">Увійти</button>
</form>
```

---

## 🔗 Інтеграція з іншими контролерами

### Admin, Teacher, Student Controllers:

```csharp
[Authorize(Roles = "Admin")]
public class AdminController : Controller
{
    // User.Identity.IsAuthenticated = true
    // User.IsInRole("Admin") = true
    // User.FindFirstValue(ClaimTypes.NameIdentifier) = "123"
}
```

### Helper methods у контролерах:

```csharp
private async Task<StudentProfile?> GetCurrentStudentProfileAsync()
{
    var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
    
    return await _context.StudentProfiles
        .Include(s => s.User)
        .FirstOrDefaultAsync(s => s.UserId == userId);
}
```

**Працює завдяки Claims створеним в AuthController!**

---

## 📊 TempData Messages

```csharp
TempData["Success"] = "Вітаємо, Іван Іванович!";
TempData["Info"] = "Ви успішно вийшли з системи";
```

**Відображення в _Layout.cshtml:**
```cshtml
@if (TempData["Success"] != null)
{
    <div class="alert alert-success">@TempData["Success"]</div>
}
```

---

## 🎯 Сценарії використання

### Сценарій 1: Студент входить
```
1. GET /Auth/Login
2. POST /Auth/Login (email=student@rfkit.edu.ua, password=pass123)
3. UserService перевіряє → User знайдено, Role=Student
4. Створює Claims (UserId=15, Role=Student)
5. Редірект → /Student/Index (Dashboard)
6. Student бачить свій профіль та курси
```

### Сценарій 2: Викладач намагається зайти на admin панель
```
1. Teacher авторизований (Role=Teacher)
2. Переходить на /Admin/Departments
3. [Authorize(Roles = "Admin")] блокує доступ
4. Редірект → /Auth/AccessDenied?returnUrl=/Admin/Departments
5. Показує "У вас немає прав для доступу до цієї сторінки"
```

### Сценарій 3: ReturnUrl після логіну
```
1. Неавторизований користувач → /Teacher/Courses
2. Автоматичний редірект → /Auth/Login?returnUrl=%2FTeacher%2FCourses
3. POST /Auth/Login (успішно)
4. RedirectAfterLogin() бачить returnUrl
5. Редірект → /Teacher/Courses (оригінальна сторінка)
```

---

## ⚠️ Важливі моменти

### 1. Logout має бути POST
```cshtml
<!-- Правильно -->
<form asp-controller="Auth" asp-action="Logout" method="post">
    <button type="submit">Вийти</button>
</form>

<!-- Неправильно (CSRF уразливість) -->
<a href="/Auth/Logout">Вийти</a>
```

### 2. RememberMe впливає на час життя Cookie
```csharp
// RememberMe = true → 30 днів
// RememberMe = false → 8 годин
```

### 3. Claims доступні через User
```csharp
// В контролерах:
var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
var role = User.FindFirstValue(ClaimTypes.Role);
var isAdmin = User.IsInRole("Admin");
```

### 4. IsLocalUrl захищає від атак
```csharp
// Безпечно
returnUrl = "/Admin/Index" → ✅ Redirect

// Небезпечно (блокується)
returnUrl = "http://evil.com" → ❌ Не redirect, йде на Home
```

---

## 🎯 Підсумок методів

| Метод | HTTP | Призначення |
|-------|------|-------------|
| `Login()` | GET | Форма входу |
| `Login(LoginDto)` | POST | Обробка входу + Claims |
| `Logout()` | POST | Вихід з системи |
| `AccessDenied()` | GET | Сторінка відмови у доступі |
| `RedirectToHome()` | Helper | Редірект за роллю |
| `RedirectAfterLogin()` | Helper | Редірект після логіну |

**Всього:** 6 методів (4 публічні, 2 helper)

---

## 🔧 Налаштування Program.cs

### Додати сервіси:
```csharp
builder.Services.AddScoped<IUserService, UserService>();

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Auth/Login";
        options.LogoutPath = "/Auth/Logout";
        options.AccessDeniedPath = "/Auth/AccessDenied";
        options.ExpireTimeSpan = TimeSpan.FromHours(8);
        options.SlidingExpiration = true;
        options.Cookie.HttpOnly = true;
        options.Cookie.SecurePolicy = CookieSecurePolicy.Always; // HTTPS
        options.Cookie.SameSite = SameSiteMode.Lax;
    });

builder.Services.AddAuthorization();
```

### Додати middleware:
```csharp
app.UseRouting();

app.UseAuthentication();  // ДО UseAuthorization!
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
```

**Важливо:** `UseAuthentication()` має бути **ДО** `UseAuthorization()`!

---

## ✅ Все працює разом!

```
AuthController (Login)
  ↓ Створює Claims
  
Cookie з Claims (UserId=15, Role=Student)
  ↓ Автоматично в наступних запитах
  
StudentController.GetCurrentStudentProfileAsync()
  ↓ Отримує UserId з Claims
  
var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
  ↓ userId = 15
  
Завантажує StudentProfile з БД
  ↓
  
Показує профіль студента
```

**Все зав'язано на Claims створених в AuthController.Login()!** 🎯

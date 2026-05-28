# 📦 Services - Повний огляд сервісів

## ✅ Створені сервіси (3/3 обов'язкових)

### 1. **UserService** ✅
**Файли:**
- [IUserService.cs](Interfaces/IUserService.cs)
- [UserService.cs](UserService.cs)
- [USERSERVICE_USAGE_EXAMPLES.cs](USERSERVICE_USAGE_EXAMPLES.cs)

**Призначення:** Управління користувачами системи

**Патерн:** Factory Method Pattern (створення User + Profile)

**Методи:**
```csharp
// Створення користувачів
Task<StudentProfile> CreateStudentAsync(name, email, password, groupId)
Task<TeacherProfile> CreateTeacherAsync(name, email, password, departmentId)
Task<User> CreateAdminAsync(name, email, password)

// Аутентифікація
Task<User?> AuthenticateAsync(email, password)

// Управління профілем
Task ChangePasswordAsync(userId, oldPassword, newPassword)
Task<bool> EmailExistsAsync(email)
Task<StudentProfile?> GetStudentProfileAsync(userId)
Task<TeacherProfile?> GetTeacherProfileAsync(userId)
Task UpdateUserProfileAsync(userId, name, email)

// Видалення
Task DeleteUserAsync(userId)
```

**Використовується в:**
- AuthController.Login() - аутентифікація
- AdminController.CreateStudent() - створення студентів
- AdminController.CreateTeacher() - створення викладачів

---

### 2. **FileStorageService** ✅
**Файли:**
- [IFileStorageService.cs](Interfaces/IFileStorageService.cs)
- [FileStorageService.cs](FileStorageService.cs)

**Призначення:** Робота з файловою системою

**Папка збереження:** `wwwroot/uploads/{folder}/`

**Методи:**
```csharp
// Основні операції
Task<string> SaveFileAsync(IFormFile file, string folder)
Task<byte[]> GetFileAsync(string filePath)
Task DeleteFileAsync(string filePath)

// Допоміжні
bool FileExists(string filePath)
long GetFileSize(string filePath)
string GetContentType(string fileName)
```

**Особливості:**
- ✅ Генерує унікальні імена файлів (Guid)
- ✅ Обмеження розміру: 10 MB
- ✅ Підтримка MIME-типів (PDF, DOCX, ZIP, тощо)
- ✅ Автоматичне створення папок
- ✅ Повертає відносний шлях для БД

**Використовується в:**
- TeacherController - завантаження файлів завдань (AssignmentFile)
- StudentController - завантаження робіт студентів (Submission.FilePath)

**Приклад використання:**
```csharp
// Завантаження файлу завдання
var filePath = await _fileStorageService.SaveFileAsync(file, "assignments");
// Повертає: "uploads/assignments/guid_filename.pdf"

// Отримання файлу
var fileBytes = await _fileStorageService.GetFileAsync(filePath);
return File(fileBytes, _fileStorageService.GetContentType(fileName), fileName);
```

---

### 3. **EmailService** ✅
**Файли:**
- [IEmailService.cs](Interfaces/IEmailService.cs)
- [EmailService.cs](EmailService.cs)

**Призначення:** Відправка email повідомлень

**Протокол:** SMTP (Gmail, тощо)

**Методи:**
```csharp
// Загальна відправка
Task SendEmailAsync(string to, string subject, string body)

// Спеціалізовані методи
Task SendCredentialsAsync(to, name, email, password, role)
Task SendNewCourseNotificationAsync(to, studentName, courseName, teacherName)
Task SendNewAssignmentNotificationAsync(to, studentName, assignmentTitle, dueDate)
Task SendGradeNotificationAsync(to, studentName, assignmentTitle, points, maxPoints, feedback)
```

**Особливості:**
- ✅ HTML-шаблони листів з стилями
- ✅ Якщо SMTP не налаштовано - логує замість відправки
- ✅ Обробка помилок з логуванням

**Використовується в:**
- AdminController.CreateStudent() - надсилання credentials (UC5)
- AdminController.CreateTeacher() - надсилання credentials (UC6)
- TeacherController.CreateAssignment() - опціонально (сповіщення студентів)
- TeacherController.GradeSubmission() - опціонально (сповіщення про оцінку)

**Налаштування в appsettings.json:**
```json
{
  "Email": {
    "SmtpHost": "smtp.gmail.com",
    "SmtpPort": 587,
    "Username": "your-email@gmail.com",
    "Password": "your-app-password",
    "FromEmail": "noreply@myracit.edu.ua",
    "FromName": "My RACIT System"
  }
}
```

**Приклад використання:**
```csharp
// Надсилання credentials студенту
await _emailService.SendCredentialsAsync(
    student.User.Email,
    student.User.Name,
    student.User.Email,
    generatedPassword,
    "Student"
);

// Повідомлення про оцінку
await _emailService.SendGradeNotificationAsync(
    student.User.Email,
    student.User.Name,
    assignment.Title,
    grade.Points,
    assignment.MaxPoints,
    grade.Feedback
);
```

---

## 🔵 Опціональні сервіси (Nice to have)

### CourseService (не реалізовано)
**Призначення:** Бізнес-логіка курсів

**Можливі методи:**
```csharp
Task<Course> CreateCourseAsync(subjectId, teacherId, groupId, startDate, endDate)
Task EnrollStudentsAsync(courseId) // Автоматичне записування студентів групи
Task<List<Student>> GetCourseStudentsAsync(courseId)
Task<CourseStatistics> GetCourseStatisticsAsync(courseId)
```

**Чому не критично:** Логіка вже в AdminController.CreateCourse()

---

### AssignmentService (не реалізовано)
**Призначення:** Бізнес-логіка завдань

**Можливі методи:**
```csharp
Task<Assignment> CreateAssignmentAsync(courseId, title, description, dueDate, maxPoints)
Task AddFilesToAssignmentAsync(assignmentId, files)
Task AddLinksToAssignmentAsync(assignmentId, links)
Task NotifyStudentsAsync(assignmentId) // Відправка email всім студентам
```

**Чому не критично:** Логіка вже в TeacherController

---

### SubmissionService (не реалізовано)
**Призначення:** Бізнес-логіка подання робіт

**Можливі методи:**
```csharp
Task<Submission> SubmitWorkAsync(assignmentId, studentId, content, file)
Task<Submission> ResubmitWorkAsync(submissionId, content, file)
Task<bool> IsOverdueAsync(assignmentId)
```

**Чому не критично:** Логіка вже в StudentController

---

### GradeService (не реалізовано)
**Призначення:** Бізнес-логіка оцінювання

**Можливі методи:**
```csharp
Task<Grade> GradeSubmissionAsync(submissionId, points, feedback)
Task<double> CalculateAverageGradeAsync(studentId)
Task<double> CalculateCourseAverageAsync(studentId, courseId)
Task NotifyStudentAsync(gradeId) // Email про оцінку
```

**Чому не критично:** Логіка вже в TeacherController

---

### StatisticsService (не реалізовано)
**Призначення:** Підрахунок статистики для Dashboard

**Можливі методи:**
```csharp
Task<AdminStatistics> GetAdminStatisticsAsync()
Task<TeacherStatistics> GetTeacherStatisticsAsync(teacherId)
Task<StudentStatistics> GetStudentStatisticsAsync(studentId)
```

**Чому не критично:** Статистика розраховується безпосередньо в контролерах

---

## 📊 Реєстрація сервісів у Program.cs

**Що потрібно додати:**

```csharp
using Microsoft.AspNetCore.Authentication.Cookies;

var builder = WebApplication.CreateBuilder(args);

// DbContext
builder.Services.AddDbContext<MyRacitDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// ✅ Сервіси (ДОДАТИ)
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IFileStorageService, FileStorageService>();
builder.Services.AddScoped<IEmailService, EmailService>();

// ✅ Authentication (ДОДАТИ)
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Auth/Login";
        options.LogoutPath = "/Auth/Logout";
        options.AccessDeniedPath = "/Auth/AccessDenied";
        options.ExpireTimeSpan = TimeSpan.FromHours(8);
        options.SlidingExpiration = true;
        options.Cookie.HttpOnly = true;
        options.Cookie.SameSite = SameSiteMode.Lax;
    });

builder.Services.AddAuthorization();

builder.Services.AddControllersWithViews();

var app = builder.Build();

// Configure middleware
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

// ✅ ДОДАТИ Authentication ПЕРЕД Authorization
app.UseAuthentication();  // ⚠️ Критично!
app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();
```

---

## 📁 Структура сервісів

```
Services/
├── Interfaces/
│   ├── IUserService.cs           ✅
│   ├── IFileStorageService.cs    ✅
│   └── IEmailService.cs          ✅
│
├── UserService.cs                ✅
├── FileStorageService.cs         ✅
├── EmailService.cs               ✅
└── USERSERVICE_USAGE_EXAMPLES.cs ✅ (документація)
```

---

## 🎯 Використання в контролерах

### AdminController:
```csharp
public class AdminController : Controller
{
    private readonly IUserService _userService;
    private readonly IEmailService _emailService;  // Опціонально
    private readonly MyRacitDbContext _context;
    
    // Створення студента
    var student = await _userService.CreateStudentAsync(name, email, password, groupId);
    await _emailService.SendCredentialsAsync(email, name, email, password, "Student");
}
```

### TeacherController:
```csharp
public class TeacherController : Controller
{
    private readonly IFileStorageService _fileStorageService;
    private readonly IEmailService _emailService;  // Опціонально
    private readonly MyRacitDbContext _context;
    
    // Завантаження файлу завдання
    [HttpPost]
    public async Task<IActionResult> UploadFile(IFormFile file, int assignmentId)
    {
        var filePath = await _fileStorageService.SaveFileAsync(file, "assignments");
        
        var assignmentFile = new AssignmentFile
        {
            AssignmentId = assignmentId,
            FilePath = filePath,
            FileName = file.FileName,
            UploadedAt = DateTime.Now
        };
        
        _context.AssignmentFiles.Add(assignmentFile);
        await _context.SaveChangesAsync();
    }
    
    // Завантаження файлу
    [HttpGet]
    public async Task<IActionResult> DownloadFile(string filePath, string fileName)
    {
        var fileBytes = await _fileStorageService.GetFileAsync(filePath);
        var contentType = _fileStorageService.GetContentType(fileName);
        return File(fileBytes, contentType, fileName);
    }
}
```

### StudentController:
```csharp
public class StudentController : Controller
{
    private readonly IFileStorageService _fileStorageService;
    private readonly MyRacitDbContext _context;
    
    // Подання роботи з файлом
    [HttpPost]
    public async Task<IActionResult> SubmitAssignment(int assignmentId, string content, IFormFile? file)
    {
        string? filePath = null;
        
        if (file != null)
        {
            filePath = await _fileStorageService.SaveFileAsync(file, "submissions");
        }
        
        var submission = new Submission
        {
            AssignmentId = assignmentId,
            StudentProfileId = student.Id,
            Content = content,
            FilePath = filePath,
            SubmittedAt = DateTime.Now
        };
        
        _context.Submissions.Add(submission);
        await _context.SaveChangesAsync();
    }
}
```

### AuthController:
```csharp
public class AuthController : Controller
{
    private readonly IUserService _userService;
    
    // Login
    var user = await _userService.AuthenticateAsync(email, password);
}
```

---

## ✅ Підсумок

### Створено:
- ✅ **UserService** - управління користувачами (Factory Method)
- ✅ **FileStorageService** - робота з файлами (завдання + роботи)
- ✅ **EmailService** - відправка email (credentials + notifications)

### Готовність:
- **Interfaces:** 3/3 ✅
- **Implementations:** 3/3 ✅
- **Documentation:** 1/1 ✅

### Залишилось:
- ⏳ Зареєструвати сервіси в Program.cs
- ⏳ Додати налаштування Email у appsettings.json (опціонально)
- ⏳ Створити папку wwwroot/uploads
- ⏳ Інтегрувати FileStorageService у контролери (додати завантаження файлів у Views)

---

## 🎉 Всі критичні сервіси створені!

**Backend сервіси: 100% ✅**

Тепер можна:
1. Налаштувати Program.cs (додати Authentication + сервіси)
2. Застосувати migration
3. Створити Views
4. Інтегрувати завантаження файлів у форми

---

**Створено:** 29 травня 2026  
**Статус:** Всі обов'язкові сервіси готові! 🚀

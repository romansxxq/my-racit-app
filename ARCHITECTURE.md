# Технічна документація MyRACIT

## Архітектура системи

### Технологічний стек
- **Backend Framework:** ASP.NET Core MVC (.NET 11.0)
- **ORM:** Entity Framework Core
- **Database:** Microsoft SQL Server
- **Admin Panel:** CoreAdmin
- **Frontend:** Razor Views, Bootstrap 5, jQuery
- **Authentication:** ASP.NET Core Identity (планується)

---

## Структура бази даних

### Таблиці та їх призначення

#### 1. Users (Користувачі)
```sql
CREATE TABLE Users (
    Id INT PRIMARY KEY IDENTITY(1,1),
    PasswordHash NVARCHAR(MAX) NOT NULL,
    Name NVARCHAR(200) NOT NULL,
    Email NVARCHAR(200) NOT NULL UNIQUE,
    Role INT NOT NULL, -- 0=Admin, 1=Teacher, 2=Student
    CONSTRAINT CHK_Email CHECK (Email LIKE '%@%')
);
```

**Призначення:** Зберігає базову інформацію про всіх користувачів системи.  
**Індекси:** Унікальний на Email для швидкого пошуку при авторизації.

---

#### 2. StudentProfiles (Профілі студентів)
```sql
CREATE TABLE StudentProfiles (
    Id INT PRIMARY KEY IDENTITY(1,1),
    UserId INT NOT NULL FOREIGN KEY REFERENCES Users(Id) ON DELETE CASCADE,
    GroupId INT NOT NULL FOREIGN KEY REFERENCES Groups(Id),
    CONSTRAINT UQ_StudentUser UNIQUE(UserId)
);
```

**Призначення:** Розширена інформація про студента, прив'язка до групи.  
**Обмеження:** Один User може мати лише один StudentProfile.

---

#### 3. TeacherProfiles (Профілі викладачів)
```sql
CREATE TABLE TeacherProfiles (
    Id INT PRIMARY KEY IDENTITY(1,1),
    UserId INT NOT NULL FOREIGN KEY REFERENCES Users(Id) ON DELETE CASCADE,
    DepartmentId INT NOT NULL FOREIGN KEY REFERENCES Departments(Id),
    CONSTRAINT UQ_TeacherUser UNIQUE(UserId)
);
```

**Призначення:** Розширена інформація про викладача, прив'язка до кафедри.  
**Обмеження:** Один User може мати лише один TeacherProfile.

---

#### 4. Departments (Кафедри)
```sql
CREATE TABLE Departments (
    Id INT PRIMARY KEY IDENTITY(1,1),
    Name NVARCHAR(200) NOT NULL,
    Code NVARCHAR(20) NOT NULL UNIQUE
);
```

**Призначення:** Довідник кафедр закладу.  
**Приклади:** "Кафедра інформаційних технологій", код "КІТ".

---

#### 5. Specialties (Спеціальності)
```sql
CREATE TABLE Specialties (
    Id INT PRIMARY KEY IDENTITY(1,1),
    Name NVARCHAR(200) NOT NULL,
    Code NVARCHAR(20) NOT NULL UNIQUE,
    DepartmentId INT NOT NULL FOREIGN KEY REFERENCES Departments(Id)
);
```

**Призначення:** Спеціальності, що належать до кафедр.  
**Приклади:** "Інженерія програмного забезпечення", код "121".

---

#### 6. Groups (Академічні групи)
```sql
CREATE TABLE Groups (
    Id INT PRIMARY KEY IDENTITY(1,1),
    Name NVARCHAR(20) NOT NULL UNIQUE,
    StudyYear INT NOT NULL CHECK (StudyYear BETWEEN 1 AND 4),
    SpecialtyId INT NOT NULL FOREIGN KEY REFERENCES Specialties(Id)
);
```

**Призначення:** Академічні групи студентів.  
**Приклади:** "ІПЗ-21", "КН-22", StudyYear=2.

---

#### 7. Subjects (Дисципліни)
```sql
CREATE TABLE Subjects (
    Id INT PRIMARY KEY IDENTITY(1,1),
    Title NVARCHAR(200) NOT NULL,
    Code NVARCHAR(20) NOT NULL UNIQUE,
    Description NVARCHAR(MAX) NULL,
    Credits INT NOT NULL CHECK (Credits > 0),
    DepartmentId INT NOT NULL FOREIGN KEY REFERENCES Departments(Id)
);
```

**Призначення:** Довідник дисциплін.  
**Приклади:** "Програмування на C#", код "CS101", 6 кредитів.

---

#### 8. Courses (Курси / Семестрові заняття)
```sql
CREATE TABLE Courses (
    Id INT PRIMARY KEY IDENTITY(1,1),
    SubjectId INT NOT NULL FOREIGN KEY REFERENCES Subjects(Id),
    TeacherId INT NOT NULL FOREIGN KEY REFERENCES TeacherProfiles(Id),
    GroupId INT NOT NULL FOREIGN KEY REFERENCES Groups(Id),
    StartDate DATE NOT NULL,
    EndDate DATE NOT NULL,
    CONSTRAINT CHK_CourseDates CHECK (EndDate > StartDate)
);
```

**Призначення:** Конкретний курс = дисципліна + викладач + група + семестр.  
**Логіка:** Один викладач веде одну дисципліну для однієї групи протягом семестру.

---

#### 9. Assignments (Завдання)
```sql
CREATE TABLE Assignments (
    Id INT PRIMARY KEY IDENTITY(1,1),
    Title NVARCHAR(200) NOT NULL,
    Description NVARCHAR(MAX) NULL,
    Deadline DATETIME NOT NULL,
    MaxGrade INT NOT NULL CHECK (MaxGrade BETWEEN 0 AND 5),
    CourseId INT NOT NULL FOREIGN KEY REFERENCES Courses(Id) ON DELETE CASCADE
);
```

**Призначення:** Завдання, створені викладачем для курсу.  
**Приклади:** "Лабораторна робота №1", MaxGrade=5, Deadline='2026-06-15'.

---

#### 10. Submissions (Подання робіт)
```sql
CREATE TABLE Submissions (
    Id INT PRIMARY KEY IDENTITY(1,1),
    StudentId INT NOT NULL FOREIGN KEY REFERENCES StudentProfiles(Id),
    AssignmentId INT NOT NULL FOREIGN KEY REFERENCES Assignments(Id) ON DELETE CASCADE,
    SubmittedAt DATETIME NOT NULL DEFAULT GETUTCDATE(),
    Content NVARCHAR(MAX) NULL,
    FilePath NVARCHAR(500) NULL,
    CONSTRAINT UQ_StudentAssignment UNIQUE(StudentId, AssignmentId)
);
```

**Призначення:** Роботи, подані студентами.  
**Обмеження:** Студент може подати роботу на завдання лише один раз (можна додати редагування).

---

#### 11. Grades (Оцінки)
```sql
CREATE TABLE Grades (
    Id INT PRIMARY KEY IDENTITY(1,1),
    StudentId INT NOT NULL FOREIGN KEY REFERENCES StudentProfiles(Id),
    AssignmentId INT NOT NULL FOREIGN KEY REFERENCES Assignments(Id) ON DELETE CASCADE,
    Value INT NOT NULL,
    DateIssued DATETIME NOT NULL DEFAULT GETUTCDATE(),
    Feedback NVARCHAR(MAX) NULL,
    CONSTRAINT UQ_StudentAssignmentGrade UNIQUE(StudentId, AssignmentId)
);
```

**Призначення:** Оцінки, виставлені викладачем за завдання.  
**Обмеження:** Одна оцінка на студента за завдання.

---

## Бізнес-логіка

### Процес створення курсу
```csharp
// 1. Адміністратор обирає дисципліну з довідника Subjects
// 2. Обирає викладача з TeacherProfiles
// 3. Обирає групу з Groups
// 4. Вказує дати семестру
// 5. Система створює Course

public async Task<Course> CreateCourseAsync(int subjectId, int teacherId, int groupId, 
    DateTime startDate, DateTime endDate)
{
    var course = new Course
    {
        SubjectId = subjectId,
        TeacherId = teacherId,
        GroupId = groupId,
        StartDate = startDate,
        EndDate = endDate
    };
    
    _context.Courses.Add(course);
    await _context.SaveChangesAsync();
    return course;
}
```

---

### Процес виставлення оцінки
```csharp
// 1. Викладач переглядає Submissions для свого Assignment
// 2. Вибирає роботу студента
// 3. Вводить оцінку та коментар
// 4. Система створює Grade

public async Task<Grade> CreateGradeAsync(int studentId, int assignmentId, 
    int value, string feedback)
{
    // Перевірка що оцінка в межах MaxGrade завдання
    var assignment = await _context.Assignments.FindAsync(assignmentId);
    if (value > assignment.MaxGrade)
        throw new ValidationException("Оцінка перевищує максимальний бал");
    
    var grade = new Grade
    {
        StudentId = studentId,
        AssignmentId = assignmentId,
        Value = value,
        DateIssued = DateTime.UtcNow,
        Feedback = feedback
    };
    
    _context.Grades.Add(grade);
    await _context.SaveChangesAsync();
    return grade;
}
```

---

### Обчислення середнього балу студента
```csharp
public async Task<double> GetStudentAverageGradeAsync(int studentId, int? courseId = null)
{
    var query = _context.Grades
        .Include(g => g.Assignment)
        .ThenInclude(a => a.Course)
        .Where(g => g.StudentId == studentId);
    
    if (courseId.HasValue)
        query = query.Where(g => g.Assignment.Course.Id == courseId.Value);
    
    var grades = await query.ToListAsync();
    
    if (!grades.Any())
        return 0;
    
    // Середній бал з урахуванням максимальних балів
    double totalPercentage = grades.Sum(g => 
        (double)g.Value / g.Assignment.MaxGrade * 100);
    
    return totalPercentage / grades.Count;
}
```

---

## Правила безпеки та авторизації

### Ролі та дозволи

#### Адміністратор (Admin):
- ✅ Повний доступ до всіх довідників
- ✅ Створення/редагування/видалення користувачів
- ✅ Створення курсів
- ✅ Перегляд всіх даних
- ❌ Не виставляє оцінки (це роль викладача)

#### Викладач (Teacher):
- ✅ Перегляд своїх курсів
- ✅ Створення завдань для своїх курсів
- ✅ Перегляд робіт студентів своїх курсів
- ✅ Виставлення оцінок студентам своїх курсів
- ❌ Не бачить курси інших викладачів
- ❌ Не редагує довідники

#### Студент (Student):
- ✅ Перегляд своїх курсів
- ✅ Перегляд завдань своїх курсів
- ✅ Подання робіт
- ✅ Перегляд своїх оцінок
- ❌ Не бачить роботи інших студентів
- ❌ Не бачить оцінки інших студентів

---

### Авторизація на рівні контролерів

```csharp
// HomeController - доступний всім авторизованим
[Authorize]
public class HomeController : Controller
{
    public IActionResult Index() => View();
}

// AdminController - лише для адміністраторів
[Authorize(Roles = "Admin")]
public class AdminController : Controller
{
    public IActionResult Dashboard() => View();
}

// TeacherController - лише для викладачів
[Authorize(Roles = "Teacher")]
public class TeacherController : Controller
{
    public IActionResult MyCourses() => View();
}

// StudentController - лише для студентів
[Authorize(Roles = "Student")]
public class StudentController : Controller
{
    public IActionResult MyCourses() => View();
}
```

---

### Валідація доступу до даних

```csharp
// Перевірка що викладач може редагувати лише свої курси
public async Task<IActionResult> EditAssignment(int id)
{
    var assignment = await _context.Assignments
        .Include(a => a.Course)
        .ThenInclude(c => c.Teacher)
        .FirstOrDefaultAsync(a => a.Id == id);
    
    if (assignment == null)
        return NotFound();
    
    // Отримуємо ID поточного викладача
    var teacherId = GetCurrentTeacherId();
    
    if (assignment.Course.TeacherId != teacherId)
        return Forbid(); // 403 Forbidden
    
    return View(assignment);
}
```

---

## Оптимізація продуктивності

### Індекси бази даних

```sql
-- Швидкий пошук користувача по email
CREATE UNIQUE INDEX IX_Users_Email ON Users(Email);

-- Пошук курсів викладача
CREATE INDEX IX_Courses_TeacherId ON Courses(TeacherId);

-- Пошук курсів групи
CREATE INDEX IX_Courses_GroupId ON Courses(GroupId);

-- Пошук оцінок студента
CREATE INDEX IX_Grades_StudentId ON Grades(StudentId);

-- Пошук завдань курсу
CREATE INDEX IX_Assignments_CourseId ON Assignments(CourseId);

-- Пошук поданих робіт за студентом
CREATE INDEX IX_Submissions_StudentId ON Submissions(StudentId);

-- Композитний індекс для унікальності
CREATE UNIQUE INDEX IX_Submissions_Student_Assignment 
    ON Submissions(StudentId, AssignmentId);
```

---

### Eager Loading в EF Core

```csharp
// Завантаження курсів з усіма зв'язками
public async Task<List<Course>> GetCoursesWithDetailsAsync()
{
    return await _context.Courses
        .Include(c => c.Subject)
        .Include(c => c.Teacher)
            .ThenInclude(t => t.User)
        .Include(c => c.Teacher)
            .ThenInclude(t => t.Department)
        .Include(c => c.Group)
            .ThenInclude(g => g.Specialty)
        .ToListAsync();
}

// Завантаження оцінок студента з деталями
public async Task<List<Grade>> GetStudentGradesAsync(int studentId)
{
    return await _context.Grades
        .Include(g => g.Assignment)
            .ThenInclude(a => a.Course)
                .ThenInclude(c => c.Subject)
        .Include(g => g.Student)
            .ThenInclude(s => s.User)
        .Where(g => g.StudentId == studentId)
        .OrderByDescending(g => g.DateIssued)
        .ToListAsync();
}
```

---

## Patterns використані в проєкті

### 1. Repository Pattern (планується)
```csharp
public interface IRepository<T> where T : class
{
    Task<T> GetByIdAsync(int id);
    Task<List<T>> GetAllAsync();
    Task<T> AddAsync(T entity);
    Task UpdateAsync(T entity);
    Task DeleteAsync(int id);
}

public class CourseRepository : IRepository<Course>
{
    private readonly MyRacitDbContext _context;
    
    public CourseRepository(MyRacitDbContext context)
    {
        _context = context;
    }
    
    // Implementation...
}
```

---

### 2. Unit of Work Pattern (планується)
```csharp
public interface IUnitOfWork : IDisposable
{
    IRepository<Course> Courses { get; }
    IRepository<Assignment> Assignments { get; }
    IRepository<Grade> Grades { get; }
    Task<int> SaveChangesAsync();
}
```

---

### 3. Service Layer Pattern
```csharp
public interface ICourseService
{
    Task<Course> CreateCourseAsync(CreateCourseDto dto);
    Task<List<Course>> GetTeacherCoursesAsync(int teacherId);
    Task<CourseDetailsDto> GetCourseDetailsAsync(int courseId);
}

public class CourseService : ICourseService
{
    private readonly MyRacitDbContext _context;
    
    public CourseService(MyRacitDbContext context)
    {
        _context = context;
    }
    
    // Business logic implementation
}
```

---

## Міграції Entity Framework

### Створення міграції
```powershell
# Створити нову міграцію
dotnet ef migrations add MigrationName

# Застосувати міграції до бази
dotnet ef database update

# Відкотити останню міграцію
dotnet ef database update PreviousMigrationName

# Видалити останню міграцію (якщо не застосована)
dotnet ef migrations remove
```

### Структура міграції
```csharp
public partial class InitialMigration : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        // Створення таблиць, індексів, обмежень
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        // Відкат змін
    }
}
```

---

## API Endpoints (REST)

### Планована REST API структура

```
GET    /api/courses              - Список курсів
GET    /api/courses/{id}         - Деталі курсу
POST   /api/courses              - Створити курс (Admin)
PUT    /api/courses/{id}         - Оновити курс (Admin)
DELETE /api/courses/{id}         - Видалити курс (Admin)

GET    /api/assignments          - Список завдань
POST   /api/assignments          - Створити завдання (Teacher)
GET    /api/assignments/{id}     - Деталі завдання

POST   /api/submissions          - Подати роботу (Student)
GET    /api/submissions/{id}     - Отримати подання

POST   /api/grades               - Виставити оцінку (Teacher)
GET    /api/grades/student/{id}  - Оцінки студента
GET    /api/grades/course/{id}   - Оцінки по курсу

GET    /api/users                - Список користувачів (Admin)
POST   /api/users                - Створити користувача (Admin)
GET    /api/users/me             - Поточний користувач
```

---

## Налаштування appsettings.json

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=MyRacitDb;Trusted_Connection=True;MultipleActiveResultSets=true"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning",
      "Microsoft.EntityFrameworkCore": "Information"
    }
  },
  "AllowedHosts": "*",
  "AppSettings": {
    "AdminEmail": "admin@rфкіт.com",
    "DefaultPassword": "ChangeMeNow!123",
    "MaxUploadSizeMB": 10,
    "SupportedFileTypes": [".pdf", ".docx", ".txt", ".zip"]
  }
}
```

---

## Розгортання (Deployment)

### 1. Локальна розробка
```powershell
# Відновити пакети
dotnet restore

# Застосувати міграції
dotnet ef database update

# Запустити проєкт
dotnet run
```

### 2. Публікація на сервер
```powershell
# Збірка для продакшену
dotnet publish -c Release -o ./publish

# Копіювання на сервер
# IIS / Kestrel / Docker
```

### 3. Docker (опціонально)
```dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:11.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:11.0 AS build
WORKDIR /src
COPY ["MyRACIT.csproj", "./"]
RUN dotnet restore
COPY . .
RUN dotnet build -c Release -o /app/build

FROM build AS publish
RUN dotnet publish -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "MyRACIT.dll"]
```

---

## Тестування

### Unit Tests
```csharp
[TestClass]
public class CourseServiceTests
{
    [TestMethod]
    public async Task CreateCourse_ValidData_ReturnsCreatedCourse()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<MyRacitDbContext>()
            .UseInMemoryDatabase("TestDb")
            .Options;
        
        using var context = new MyRacitDbContext(options);
        var service = new CourseService(context);
        
        // Act
        var course = await service.CreateCourseAsync(new CreateCourseDto
        {
            SubjectId = 1,
            TeacherId = 1,
            GroupId = 1,
            StartDate = DateTime.Now,
            EndDate = DateTime.Now.AddMonths(4)
        });
        
        // Assert
        Assert.IsNotNull(course);
        Assert.AreEqual(1, course.SubjectId);
    }
}
```

---

## Моніторинг та логування

### Serilog (планується)
```csharp
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .WriteTo.Console()
    .WriteTo.File("logs/myracit-.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

builder.Host.UseSerilog();
```

---

## Roadmap розвитку

### Фаза 1: MVP (Поточна) ✅
- ✅ Базова структура бази даних
- ✅ Моделі Entity
- ✅ Міграції
- ✅ DbContext
- 🔄 Контролери та Views

### Фаза 2: Функціонал
- ⏳ Авторизація (ASP.NET Identity)
- ⏳ CRUD для всіх сутностей
- ⏳ Виставлення оцінок
- ⏳ Подання робіт

### Фаза 3: UX/UI
- ⏳ Адаптивний дизайн
- ⏳ Dashboard для кожної ролі
- ⏳ Графіки та статистика
- ⏳ Нотифікації

### Фаза 4: Інтеграції
- ⏳ Email нотифікації
- ⏳ Експорт у Excel/PDF
- ⏳ REST API
- ⏳ Mobile-friendly версія

---

## Контакти та підтримка

**Розробник:** MyRACIT Team  
**Email:** support@myracit.com  
**GitHub:** https://github.com/myracit/myracit  
**Документація:** https://docs.myracit.com

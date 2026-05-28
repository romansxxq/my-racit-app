# Маршрут практики з ООП для MyRACIT

## 📌 Обраний варіант

**Варіант 12: Система управління навчальним закладом**

**Сутності:** студенти, викладачі, курси, групи  
**Рекомендовані патерни:**
- Composite для структури факультет/група/студент
- Visitor для обчислення рейтингів
- Factory для типів занять

---

## ✅ Поточний стан проєкту (Виконано)

### Розділ І. Архітектурні основи ООП та проєктування

#### ✅ Практичне заняття 1. Моделювання предметної області
- [x] UML-діаграми створені ([UML-diagrams.md](UML-diagrams.md))
  - Use Case діаграма (3 актори, 19 сценаріїв)
  - Class діаграма (11 Entity класів)
  - ER-діаграма
  - Sequence діаграма
  - Activity діаграма
  - Component діаграма
- [x] Базова архітектура проєкту готова
- [x] Namespace структура: `MyRACIT.Models.Entities`, `MyRACIT.Data`, `MyRACIT.Controllers`

#### 📝 Самостійна робота 1. Ініціалізація та життєвий цикл
**TODO:**
- [ ] Налаштувати Git-репозиторій
- [ ] Додати `.gitignore` для .NET проєкту
- [ ] Реалізувати конструктори з параметрами для всіх Entity
- [ ] Додати `IDisposable` де потрібно (DbContext вже має)

#### ✅ Практичне заняття 2. Інкапсуляція та цілісність даних
- [x] Private поля та public властивості у всіх Entity
- [x] Валідація через атрибути (`[Required]`, `[MaxLength]`, `[Range]`)
- [x] Інваріанти через `[EmailAddress]`, `CHECK` constraints

#### 📝 Самостійна робота 2. Агрегація даних
**TODO:**
- [ ] Додати індексатори для колекцій (наприклад `Group[index]` для студентів)
- [ ] Перевантажити оператори `==`, `!=` для Entity
- [ ] Додати оператори `+` для агрегації (наприклад, додавання студента до групи)

#### ✅ Практичне заняття 3. Наслідування та розширення поведінки
- [x] Базова ієрархія через `User` → `StudentProfile` / `TeacherProfile`
- [ ] **TODO:** Додати abstract базові класи для розширення

#### 📝 Самостійна робота 3. Перевизначення vs приховування
**TODO:**
- [ ] Додати віртуальні методи (`virtual`) у базових класах
- [ ] Перевизначити (`override`) методи `ToString()` де потрібно
- [ ] Документувати різницю між `override` та `new`

#### 🟡 Практичне заняття 4. Контрактне програмування
- [x] Entity моделі як контракти
- [ ] **TODO:** Винести інтерфейси (`IRepository<T>`, `IService<T>`)
- [ ] **TODO:** Створити абстрактні класи для спільної поведінки

#### 📝 Самостійна робота 4. Проєктування інтерфейсів взаємодії
**TODO:**
- [ ] Створити `IRepository<T>` для CRUD операцій
- [ ] Створити `IUserService`, `ICourseService`, `IGradeService`
- [ ] Забезпечити слабку зв'язність через Dependency Injection

---

## 🎯 План реалізації (Наступні кроки)

### Розділ ІІ. Структури даних, запити та обробка помилок

#### Практичне заняття 5. Узагальнені типи (Generics)
```csharp
// TODO: Створити універсальний репозиторій
public class Repository<T> where T : class
{
    private readonly MyRacitDbContext _context;
    
    public async Task<T> GetByIdAsync(int id) { }
    public async Task<List<T>> GetAllAsync() { }
    public async Task<T> AddAsync(T entity) { }
    // ...
}
```

**Файл:** `src/MyRACIT/Data/Repository.cs`

#### Самостійна робота 5. Узагальнені алгоритми
```csharp
// TODO: Extension Methods
public static class EnumerableExtensions
{
    public static void ForEach<T>(this IEnumerable<T> source, Action<T> action)
    {
        foreach (var item in source)
            action(item);
    }
}
```

#### Практичне заняття 6. Колекції .NET
**Поточне використання:**
- ✅ `List<T>` для колекцій студентів, курсів
- [ ] **TODO:** `Dictionary<int, Student>` для швидкого пошуку
- [ ] **TODO:** `HashSet<T>` для унікальних елементів (emails)

#### Практичне заняття 7. LINQ
```csharp
// TODO: Приклади LINQ запитів для MyRACIT
public async Task<List<Grade>> GetTopStudentsAsync(int courseId, int count)
{
    return await _context.Grades
        .Where(g => g.Assignment.CourseId == courseId)
        .GroupBy(g => g.StudentId)
        .Select(g => new {
            StudentId = g.Key,
            Average = g.Average(x => x.Value)
        })
        .OrderByDescending(x => x.Average)
        .Take(count)
        .ToListAsync();
}
```

#### Практичне заняття 8. Система винятків
```csharp
// TODO: Створити Custom Exceptions
public class StudentNotFoundException : Exception { }
public class InvalidGradeException : Exception { }
public class CourseFullException : Exception { }
public class DeadlineExpiredException : Exception { }
```

**Файл:** `src/MyRACIT/Exceptions/`

---

### Розділ ІІІ. SOLID та патерни проєктування

#### Практичне заняття 9. Рефакторинг за SRP та OCP

**SRP (Single Responsibility Principle):**
```
✅ User - відповідає лише за дані користувача
✅ StudentProfile - лише за профіль студента
❌ HomeController - зараз може робити багато (потрібен розділ)

TODO: Розділити контролери:
- AdminController - управління довідниками
- TeacherController - управління курсами та оцінками
- StudentController - перегляд курсів та оцінок
```

**OCP (Open-Closed Principle):**
```csharp
// TODO: Зробити систему розширюваною без модифікації
public abstract class GradeCalculator
{
    public abstract double Calculate(List<Grade> grades);
}

public class AverageGradeCalculator : GradeCalculator { }
public class WeightedGradeCalculator : GradeCalculator { }
```

#### Практичне заняття 10. Породжувальні патерни

##### 1. Factory Method - для створення користувачів
```csharp
// TODO: Файл src/MyRACIT/Factories/UserFactory.cs
public static class UserFactory
{
    public static User CreateStudent(string name, string email, int groupId)
    {
        var user = new User
        {
            Name = name,
            Email = email,
            Role = UserRole.Student,
            PasswordHash = HashPassword(GeneratePassword())
        };
        
        return user;
    }
    
    public static User CreateTeacher(string name, string email, int departmentId)
    {
        // ...
    }
    
    public static User CreateAdmin(string name, string email)
    {
        // ...
    }
}
```

##### 2. Singleton - для глобальної конфігурації
```csharp
// TODO: Файл src/MyRACIT/Services/AppConfigService.cs
public sealed class AppConfigService
{
    private static readonly Lazy<AppConfigService> _instance = 
        new Lazy<AppConfigService>(() => new AppConfigService());
    
    public static AppConfigService Instance => _instance.Value;
    
    private AppConfigService() { }
    
    public int MaxUploadSizeMB { get; set; } = 10;
    public string[] SupportedFileTypes { get; set; }
}
```

#### Практичне заняття 11. Поведінкові патерни

##### 1. Strategy - для різних алгоритмів оцінювання
```csharp
// TODO: Файл src/MyRACIT/Strategies/IGradeStrategy.cs
public interface IGradeStrategy
{
    double CalculateFinalGrade(List<Grade> grades);
}

public class AverageGradeStrategy : IGradeStrategy
{
    public double CalculateFinalGrade(List<Grade> grades)
    {
        return grades.Average(g => g.Value);
    }
}

public class WeightedGradeStrategy : IGradeStrategy
{
    public double CalculateFinalGrade(List<Grade> grades)
    {
        // Враховувати важливість завдань
    }
}

// Використання:
public class GradeService
{
    private readonly IGradeStrategy _strategy;
    
    public GradeService(IGradeStrategy strategy)
    {
        _strategy = strategy;
    }
    
    public double GetFinalGrade(int studentId, int courseId)
    {
        var grades = GetGrades(studentId, courseId);
        return _strategy.CalculateFinalGrade(grades);
    }
}
```

##### 2. Observer - для сповіщень про нові оцінки
```csharp
// TODO: Файл src/MyRACIT/Observers/IGradeObserver.cs
public interface IGradeObserver
{
    void OnGradeIssued(Grade grade);
}

public class EmailNotificationObserver : IGradeObserver
{
    public void OnGradeIssued(Grade grade)
    {
        // Відправити email студенту
        Console.WriteLine($"Email sent to {grade.Student.User.Email}");
    }
}

public class GradeStatisticsObserver : IGradeObserver
{
    public void OnGradeIssued(Grade grade)
    {
        // Оновити статистику
        Console.WriteLine("Statistics updated");
    }
}

public class GradeService
{
    private readonly List<IGradeObserver> _observers = new();
    
    public void Subscribe(IGradeObserver observer)
    {
        _observers.Add(observer);
    }
    
    public async Task IssueGradeAsync(Grade grade)
    {
        await _context.Grades.AddAsync(grade);
        await _context.SaveChangesAsync();
        
        // Сповістити всіх підписників
        foreach (var observer in _observers)
        {
            observer.OnGradeIssued(grade);
        }
    }
}
```

#### Практичне заняття 12. Структурні патерни

##### 1. Composite - для ієрархії організаційної структури
```csharp
// TODO: Файл src/MyRACIT/Models/Composite/IOrganizationalUnit.cs
public interface IOrganizationalUnit
{
    string Name { get; }
    int GetTotalStudentsCount();
    void Display(int depth = 0);
}

// Лист - окремий студент
public class StudentLeaf : IOrganizationalUnit
{
    public string Name { get; set; }
    
    public int GetTotalStudentsCount() => 1;
    
    public void Display(int depth = 0)
    {
        Console.WriteLine(new string(' ', depth * 2) + $"- {Name}");
    }
}

// Композит - група
public class GroupComposite : IOrganizationalUnit
{
    private readonly List<IOrganizationalUnit> _students = new();
    
    public string Name { get; set; }
    
    public void Add(IOrganizationalUnit unit)
    {
        _students.Add(unit);
    }
    
    public int GetTotalStudentsCount()
    {
        return _students.Sum(s => s.GetTotalStudentsCount());
    }
    
    public void Display(int depth = 0)
    {
        Console.WriteLine(new string(' ', depth * 2) + $"Група: {Name}");
        foreach (var student in _students)
        {
            student.Display(depth + 1);
        }
    }
}

// Композит - спеціальність
public class SpecialtyComposite : IOrganizationalUnit
{
    private readonly List<IOrganizationalUnit> _groups = new();
    
    public string Name { get; set; }
    
    public void Add(IOrganizationalUnit unit)
    {
        _groups.Add(unit);
    }
    
    public int GetTotalStudentsCount()
    {
        return _groups.Sum(g => g.GetTotalStudentsCount());
    }
    
    public void Display(int depth = 0)
    {
        Console.WriteLine(new string(' ', depth * 2) + $"Спеціальність: {Name}");
        foreach (var group in _groups)
        {
            group.Display(depth + 1);
        }
    }
}

// Використання:
var ipz21 = new GroupComposite { Name = "ІПЗ-21" };
ipz21.Add(new StudentLeaf { Name = "Іванов І.І." });
ipz21.Add(new StudentLeaf { Name = "Петров П.П." });

var ipz = new SpecialtyComposite { Name = "Інженерія ПЗ" };
ipz.Add(ipz21);

Console.WriteLine($"Всього студентів: {ipz.GetTotalStudentsCount()}");
ipz.Display();
```

##### 2. Decorator - для розширення функціоналу курсів
```csharp
// TODO: Файл src/MyRACIT/Decorators/ICourse.cs
public interface ICourseComponent
{
    string GetDescription();
    decimal GetCredits();
}

public class BasicCourse : ICourseComponent
{
    protected Course _course;
    
    public BasicCourse(Course course)
    {
        _course = course;
    }
    
    public virtual string GetDescription()
    {
        return _course.Subject.Title;
    }
    
    public virtual decimal GetCredits()
    {
        return _course.Subject.Credits;
    }
}

// Декоратор - лабораторні роботи
public class LabWorkDecorator : BasicCourse
{
    public LabWorkDecorator(Course course) : base(course) { }
    
    public override string GetDescription()
    {
        return base.GetDescription() + " + Лабораторні роботи";
    }
    
    public override decimal GetCredits()
    {
        return base.GetCredits() + 1; // +1 кредит за лабораторні
    }
}

// Декоратор - курсовий проєкт
public class CourseProjectDecorator : BasicCourse
{
    public CourseProjectDecorator(Course course) : base(course) { }
    
    public override string GetDescription()
    {
        return base.GetDescription() + " + Курсовий проєкт";
    }
    
    public override decimal GetCredits()
    {
        return base.GetCredits() + 2;
    }
}

// Використання:
var course = new Course { Subject = new Subject { Title = "ООП", Credits = 4 } };
var basicCourse = new BasicCourse(course);
var withLabs = new LabWorkDecorator(course);
var withProject = new CourseProjectDecorator(withLabs);

Console.WriteLine(withProject.GetDescription()); // ООП + Лабораторні + Курсовий
Console.WriteLine(withProject.GetCredits()); // 7 кредитів
```

##### 3. Facade - спрощений інтерфейс для адміністрування
```csharp
// TODO: Файл src/MyRACIT/Facades/EnrollmentFacade.cs
public class EnrollmentFacade
{
    private readonly IRepository<User> _userRepo;
    private readonly IRepository<StudentProfile> _studentRepo;
    private readonly IRepository<Course> _courseRepo;
    private readonly IRepository<Grade> _gradeRepo;
    
    public EnrollmentFacade(...)
    {
        // DI
    }
    
    // Єдина точка входу для складного процесу запису
    public async Task<bool> EnrollStudentToAllCoursesAsync(int studentId)
    {
        try
        {
            var student = await _studentRepo.GetByIdAsync(studentId);
            var courses = await GetGroupCoursesAsync(student.GroupId);
            
            foreach (var course in courses)
            {
                await CreateDefaultGradesAsync(studentId, course.Id);
            }
            
            await SendWelcomeEmailAsync(student);
            await LogEnrollmentAsync(studentId);
            
            return true;
        }
        catch
        {
            return false;
        }
    }
}
```

##### 4. Visitor - для обчислення різних метрик
```csharp
// TODO: Файл src/MyRACIT/Visitors/IStudentVisitor.cs
public interface IStudentVisitor
{
    void Visit(StudentProfile student);
    double GetResult();
}

public class AverageGradeVisitor : IStudentVisitor
{
    private double _totalGrade;
    private int _count;
    
    public void Visit(StudentProfile student)
    {
        var grades = GetStudentGrades(student.Id);
        _totalGrade += grades.Average(g => g.Value);
        _count++;
    }
    
    public double GetResult()
    {
        return _count > 0 ? _totalGrade / _count : 0;
    }
}

public class AttendanceVisitor : IStudentVisitor
{
    private int _totalClasses;
    private int _attended;
    
    public void Visit(StudentProfile student)
    {
        // Обчислити відвідуваність
    }
    
    public double GetResult()
    {
        return _totalClasses > 0 ? (double)_attended / _totalClasses * 100 : 0;
    }
}

// Використання:
public class Group
{
    private List<StudentProfile> _students;
    
    public void Accept(IStudentVisitor visitor)
    {
        foreach (var student in _students)
        {
            visitor.Visit(student);
        }
    }
}

var group = GetGroup(1);
var gradeVisitor = new AverageGradeVisitor();
group.Accept(gradeVisitor);
Console.WriteLine($"Середній бал групи: {gradeVisitor.GetResult()}");
```

---

### Розділ IV. Серіалізація, тестування та життєвий цикл

#### Практичне заняття 13. Зберігання стану (JSON)
```csharp
// TODO: Файл src/MyRACIT/Services/StateService.cs
using System.Text.Json;

public class StateService
{
    public async Task SaveStateAsync<T>(T state, string filename)
    {
        var options = new JsonSerializerOptions
        {
            WriteIndented = true,
            ReferenceHandler = ReferenceHandler.Preserve
        };
        
        var json = JsonSerializer.Serialize(state, options);
        await File.WriteAllTextAsync(filename, json);
    }
    
    public async Task<T> LoadStateAsync<T>(string filename)
    {
        var json = await File.ReadAllTextAsync(filename);
        return JsonSerializer.Deserialize<T>(json);
    }
}
```

#### Самостійна робота 13. DTO і проблеми вкладеності
```csharp
// TODO: Файл src/MyRACIT/DTOs/
public class CourseDto
{
    public int Id { get; set; }
    public string SubjectTitle { get; set; }
    public string TeacherName { get; set; }
    public string GroupName { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
}

// Маппінг
public static class CourseMappingExtensions
{
    public static CourseDto ToDto(this Course course)
    {
        return new CourseDto
        {
            Id = course.Id,
            SubjectTitle = course.Subject?.Title,
            TeacherName = course.Teacher?.User?.Name,
            GroupName = course.Group?.Name,
            StartDate = course.StartDate,
            EndDate = course.EndDate
        };
    }
}
```

#### Практичне заняття 14. Модульне тестування

```csharp
// TODO: Файл tests/MyRACIT.Tests/Services/GradeServiceTests.cs
using NUnit.Framework;
using Moq;

[TestFixture]
public class GradeServiceTests
{
    private Mock<IRepository<Grade>> _gradeRepoMock;
    private GradeService _gradeService;
    
    [SetUp]
    public void Setup()
    {
        _gradeRepoMock = new Mock<IRepository<Grade>>();
        _gradeService = new GradeService(_gradeRepoMock.Object);
    }
    
    [Test]
    public async Task CalculateAverageGrade_WithValidGrades_ReturnsCorrectAverage()
    {
        // Arrange
        var grades = new List<Grade>
        {
            new Grade { Value = 5 },
            new Grade { Value = 4 },
            new Grade { Value = 5 }
        };
        
        _gradeRepoMock
            .Setup(r => r.GetAllAsync())
            .ReturnsAsync(grades);
        
        // Act
        var result = await _gradeService.CalculateAverageAsync(1);
        
        // Assert
        Assert.AreEqual(4.67, result, 0.01);
    }
    
    [Test]
    [TestCase(0, ExpectedException = typeof(ArgumentException))]
    [TestCase(6, ExpectedException = typeof(ArgumentException))]
    public void ValidateGrade_InvalidValue_ThrowsException(int value)
    {
        // Arrange & Act & Assert
        Assert.Throws<ArgumentException>(() => 
            _gradeService.ValidateGrade(value));
    }
}
```

#### Самостійна робота 14. Ізоляція тестів за допомогою моків
```csharp
// TODO: Встановити NuGet пакет Moq
dotnet add package Moq

// Приклад тесту з моками
[Test]
public async Task CreateCourse_ValidData_CallsRepositoryAdd()
{
    // Arrange
    var courseRepoMock = new Mock<IRepository<Course>>();
    var courseService = new CourseService(courseRepoMock.Object);
    
    var course = new Course
    {
        SubjectId = 1,
        TeacherId = 1,
        GroupId = 1
    };
    
    // Act
    await courseService.CreateAsync(course);
    
    // Assert
    courseRepoMock.Verify(r => r.AddAsync(It.IsAny<Course>()), Times.Once);
}
```

#### Практичне заняття 15. Аналіз коду та рефакторинг
```
TODO:
- [ ] Усунути "магічні числа" - винести у константи
- [ ] Перевірити naming conventions (PascalCase, camelCase)
- [ ] Додати XML-коментарі до публічних методів
- [ ] Видалити невикористаний using
- [ ] Рефакторинг довгих методів (> 50 рядків)
```

#### Самостійна робота 15. Документація проєкту
- [x] ✅ README.md створено
- [x] ✅ UML-diagrams.md створено
- [x] ✅ USE-CASES.md створено
- [x] ✅ ARCHITECTURE.md створено
- [ ] TODO: Додати CHANGELOG.md
- [ ] TODO: Створити інструкцію з запуску

---

## 📊 Контрольний список патернів для MyRACIT

### Породжувальні (Creational)
- [ ] **Factory Method** - для створення користувачів різних ролей
- [ ] **Abstract Factory** - для створення груп сутностей (Course + Assignments)
- [ ] **Singleton** - для глобальної конфігурації, логування
- [ ] **Builder** - для складних об'єктів (CourseBuilder з усіма налаштуваннями)

### Структурні (Structural)
- [ ] **Composite** ⭐ - для ієрархії Department → Specialty → Group → Student
- [ ] **Decorator** - для розширення курсів (з лабораторними, з проєктом)
- [ ] **Facade** - спрощений API для адміністрування
- [ ] **Adapter** - для інтеграції зовнішніх сервісів (email, SMS)
- [ ] **Proxy** - для контролю доступу до даних

### Поведінкові (Behavioral)
- [ ] **Strategy** ⭐ - для різних алгоритмів оцінювання
- [ ] **Observer** ⭐ - для сповіщень про події (нова оцінка, дедлайн)
- [ ] **Visitor** ⭐ - для обчислення різних метрик по студентах
- [ ] **State** - для станів завдання (Новий → В роботі → Здано → Оцінено)
- [ ] **Command** - для операцій з undo/redo (видалення курсу)
- [ ] **Template Method** - для алгоритму обробки завдань
- [ ] **Iterator** - для обходу колекцій студентів, курсів

⭐ - обов'язкові патерни згідно варіанту практики

---

## 📅 Графік виконання (16 тижнів)

| Тиждень | Розділ | Завдання |
|---------|--------|----------|
| 1 | I.1 | UML-діаграми ✅ |
| 2 | I.2 | Інкапсуляція, валідація ✅ |
| 3 | I.3 | Наслідування, поліморфізм |
| 4 | I.4 | Інтерфейси, абстрактні класи |
| 5 | II.5 | Generics, Repository<T> |
| 6 | II.6 | Колекції, оптимізація |
| 7 | II.7 | LINQ запити |
| 8 | II.8 | Exceptions, Retry Policy |
| 9 | III.9 | SOLID рефакторинг |
| 10 | III.10 | Factory, Singleton |
| 11 | III.11 | Strategy, Observer |
| 12 | III.12 | Composite, Decorator, Facade |
| 13 | IV.13 | JSON серіалізація, DTO |
| 14 | IV.14 | Unit-тести, Moq |
| 15 | IV.15 | Code Review, документація |
| 16 | IV.16 | Демонстрація та захист |

---

## 🎯 Критерії успішності

### Мінімальні вимоги (3 бали):
- ✅ UML-діаграми створені
- ✅ Базова архітектура MVC
- ✅ Entity моделі з валідацією
- [ ] 3+ патерни реалізовані
- [ ] Базові unit-тести

### Достатні вимоги (4 бали):
- ✅ Все з мінімальних
- [ ] 6+ патернів реалізовані
- [ ] SOLID принципи дотримані
- [ ] Покриття тестами 50%+
- [ ] Документація повна

### Високі вимоги (5 балів):
- ✅ Все з достатніх
- [ ] 10+ патернів реалізовані
- [ ] Архітектурно чистий код
- [ ] Покриття тестами 70%+
- [ ] Демонстрація роботи
- [ ] Обґрунтування рішень

---

## 🚀 Наступні кроки (Пріоритет)

### 1. Високий пріоритет (цей тиждень)
- [ ] Створити `IRepository<T>` інтерфейс
- [ ] Реалізувати `UserFactory` (Factory Method)
- [ ] Впровадити `IGradeStrategy` + 2 реалізації
- [ ] Додати базові unit-тести

### 2. Середній пріоритет (наступний тиждень)
- [ ] Реалізувати Composite для організаційної структури
- [ ] Додати Observer для сповіщень про оцінки
- [ ] Створити Visitor для обчислення рейтингів
- [ ] Рефакторинг за SOLID

### 3. Низький пріоритет (потім)
- [ ] Decorator для курсів
- [ ] Facade для адміністрування
- [ ] Command для undo/redo
- [ ] Покриття тестами 70%+

---

## 📚 Корисні ресурси

1. **Design Patterns:** https://refactoring.guru/design-patterns
2. **SOLID Principles:** https://www.baeldung.com/solid-principles
3. **C# Best Practices:** https://learn.microsoft.com/dotnet/csharp/
4. **Unit Testing:** https://docs.nunit.org/
5. **Moq Documentation:** https://github.com/moq/moq4

---

## ✅ Підсумок

Ваш проєкт MyRACIT має відмінну базу:
- ✅ Продумана архітектура
- ✅ Повна документація (UML, Use Cases, Architecture)
- ✅ Правильна структура Entity
- ✅ EF Core міграції працюють

**Залишається:**
1. Впровадити патерни проєктування (Composite, Visitor, Factory, Strategy, Observer)
2. Написати unit-тести
3. Провести рефакторинг за SOLID
4. Підготувати демонстрацію

**Оцінка готовності:** 40% виконано ✅  
**Прогноз:** При систематичній роботі проєкт буде готовий за 8-10 тижнів! 🎉

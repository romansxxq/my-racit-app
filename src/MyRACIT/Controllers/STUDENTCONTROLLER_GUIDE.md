# StudentController - Довідник методів

## 📊 Загальна інформація

**Роль доступу:** `[Authorize(Roles = "Student")]`  
**Use Cases:** UC13-UC17  
**Залежності:** `MyRacitDbContext`

---

## 🔐 Аутентифікація студента

### `GetCurrentStudentProfileAsync()`
**Приватний helper метод**

Отримує профіль поточного авторизованого студента:
```csharp
var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
return await _context.StudentProfiles
    .Include(s => s.User)
    .Include(s => s.Group)
        .ThenInclude(g => g!.Specialty)
    .FirstOrDefaultAsync(s => s.UserId == userId);
```

Використовується в **кожному** методі для перевірки доступу.

---

## 👤 UC13: ПЕРЕГЛЯД ПРОФІЛЮ

### `Index()` / `Profile()`
- **GET** `/Student/Index`
- Головна сторінка студента (Dashboard)
- Показує профіль + статистику

**Статистика у ViewBag:**
```csharp
ViewBag.TotalCourses = 5;           // Курсів у групі
ViewBag.TotalSubmissions = 18;      // Всього поданих робіт
ViewBag.TotalGrades = 12;           // Оцінених робіт
ViewBag.AverageGrade = 87.5;        // Середній бал
```

**Дані:**
- StudentProfile → User
- StudentProfile → Group → Specialty
- Статистика з підрахунками

---

## 📚 UC14: ПЕРЕГЛЯД ДОСТУПНИХ КУРСІВ

### `MyCourses()`
- **GET** `/Student/MyCourses`
- Список курсів групи студента
- Курси доступні через `GroupId`

**Дані:**
```csharp
var courses = await _context.Courses
    .Include(c => c.Subject)
    .Include(c => c.Teacher)
        .ThenInclude(t => t!.User)
    .Include(c => c.Group)
    .Where(c => c.GroupId == student.GroupId)
    .OrderByDescending(c => c.StartDate)
    .ToListAsync();
```

**ViewBag:**
- `StudentName` - ім'я студента
- `GroupName` - назва групи

---

### `CourseDetails(id)`
- **GET** `/Student/CourseDetails/5`
- Деталі курсу з завданнями та моїми роботами
- Перевірка: `course.GroupId == student.GroupId`

**ViewBag дані:**
- `Assignments` - всі завдання курсу
- `MySubmissions` - мої подані роботи (з Grade)

**Дані:**
- Course → Subject
- Course → Teacher → User + Department
- Course → Group

---

## 📝 UC15: ПЕРЕГЛЯД ЗАВДАНЬ КУРСУ

### `Assignments(courseId)`
- **GET** `/Student/Assignments?courseId=5`
- Список завдань курсу з моїм статусом

**Запит з підзапитом:**
```csharp
var assignments = await _context.Assignments
    .Where(a => a.CourseId == courseId)
    .Select(a => new
    {
        Assignment = a,
        MySubmission = _context.Submissions
            .Include(s => s.Grade)
            .FirstOrDefault(s => s.AssignmentId == a.Id 
                              && s.StudentProfileId == student.Id)
    })
    .ToListAsync();
```

**Статуси в UI:**
- ✅ **Оцінено** - MySubmission.Grade != null
- ⏳ **Очікує перевірки** - MySubmission != null && Grade == null
- ❌ **Не подано** - MySubmission == null

---

### `AssignmentDetails(id)`
- **GET** `/Student/AssignmentDetails/5`
- Деталі завдання з файлами, посиланнями, моєю роботою

**ViewBag дані:**
- `Files` - AssignmentFiles
- `Links` - AssignmentLinks
- `MySubmission` - моя робота (якщо подана)
- `IsOverdue` - чи прострочено дедлайн
- `DaysUntilDue` - днів до дедлайну

**Приклад перевірки дедлайну:**
```csharp
var now = DateTime.Now;
ViewBag.IsOverdue = assignment.DueDate < now;
ViewBag.DaysUntilDue = (assignment.DueDate - now).Days;
```

---

## 📥 UC16: ПОДАННЯ РОБОТИ

### `SubmitAssignment(assignmentId)` - GET
- **GET** `/Student/SubmitAssignment?assignmentId=5`
- Форма подання роботи
- Перевіряє чи вже подана робота (для переподання)

**Логіка:**
```csharp
var existingSubmission = await _context.Submissions
    .FirstOrDefaultAsync(s => s.AssignmentId == assignmentId 
                           && s.StudentProfileId == student.Id);

if (existingSubmission != null)
{
    TempData["Info"] = "Ви вже подали роботу. Можете переподати.";
}
```

---

### `SubmitAssignment(Submission)` - POST
- **POST** `/Student/SubmitAssignment`
- Зберігає нову роботу або переподає існуючу

**Логіка переподання:**
```csharp
if (existingSubmission != null)
{
    // Переподання
    existingSubmission.Content = submission.Content;
    existingSubmission.FilePath = submission.FilePath;
    existingSubmission.SubmittedAt = DateTime.Now;
    
    // Видаляємо стару оцінку
    if (existingSubmission.Grade != null)
    {
        _context.Grades.Remove(existingSubmission.Grade);
        TempData["Info"] = "Попередня оцінка видалена.";
    }
}
else
{
    // Нова робота
    submission.StudentProfileId = student.Id;
    submission.SubmittedAt = DateTime.Now;
    _context.Submissions.Add(submission);
}
```

**Редірект:** `MySubmissions` після збереження

---

### `MySubmissions(courseId)`
- **GET** `/Student/MySubmissions?courseId=5`
- Список моїх поданих робіт по курсу
- Сортування: за датою подання (найновіші зверху)

**Дані:**
```csharp
var submissions = await _context.Submissions
    .Include(s => s.Assignment)
    .Include(s => s.Grade)
    .Where(s => s.StudentProfileId == student.Id 
             && s.Assignment!.CourseId == courseId)
    .OrderByDescending(s => s.SubmittedAt)
    .ToListAsync();
```

---

### `SubmissionDetails(id)`
- **GET** `/Student/SubmissionDetails/5`
- Детальна інформація про мою роботу
- Показує: контент, файл, оцінку (якщо є), фідбек

**Дані:**
- Submission → Assignment → Course → Subject
- Submission → Grade

---

### `DeleteSubmission(id)`
- **POST** `/Student/DeleteSubmission/5`
- Видалення моєї роботи

**Валідація:**
```csharp
if (submission.Grade != null)
{
    TempData["Error"] = "Неможливо видалити оцінену роботу!";
    return RedirectToAction(nameof(SubmissionDetails), new { id });
}
```

**Логіка:** Можна видалити тільки неоцінені роботи

---

## 📊 UC17: ПЕРЕГЛЯД СВОЇХ ОЦІНОК

### `MyGrades()`
- **GET** `/Student/MyGrades`
- Всі оцінки студента згруповані по курсах
- Показує загальну статистику

**Дані:**
```csharp
var grades = await _context.Grades
    .Include(g => g.Submission)
        .ThenInclude(s => s!.Assignment)
            .ThenInclude(a => a!.Course)
                .ThenInclude(c => c!.Subject)
    .Where(g => g.StudentProfileId == student.Id)
    .OrderByDescending(g => g.GradedAt)
    .ToListAsync();

// Групування
var gradesByCourse = grades
    .GroupBy(g => g.Submission!.Assignment!.Course)
    .ToList();
```

**Статистика у ViewBag:**
```csharp
ViewBag.TotalGrades = 15;              // Оцінених робіт
ViewBag.AverageGrade = 87.5;           // Середній бал
ViewBag.TotalPoints = 1312;            // Набрано балів
ViewBag.MaxPossiblePoints = 1500;      // Максимум можливих
ViewBag.Percentage = 87.47;            // Відсоток виконання
```

---

### `CourseGrades(courseId)`
- **GET** `/Student/CourseGrades?courseId=5`
- Оцінки студента по одному курсу
- Показує всі завдання + статус виконання

**Запит з підзапитом:**
```csharp
var assignments = await _context.Assignments
    .Where(a => a.CourseId == courseId)
    .Select(a => new
    {
        Assignment = a,
        MySubmission = _context.Submissions
            .Include(s => s.Grade)
            .FirstOrDefault(s => s.AssignmentId == a.Id 
                              && s.StudentProfileId == student.Id)
    })
    .ToListAsync();
```

**Статистика курсу:**
```csharp
ViewBag.TotalAssignments = 10;           // Всього завдань
ViewBag.CompletedAssignments = 7;        // Оцінено (70%)
ViewBag.PendingAssignments = 2;          // Очікує перевірки (20%)
ViewBag.NotSubmittedAssignments = 1;     // Не подано (10%)
ViewBag.CourseAverage = 85.3;            // Середній % по курсу
```

---

## 🔒 Безпека

### Перевірка доступу в кожному методі:

1. **Отримати профіль студента:**
   ```csharp
   var student = await GetCurrentStudentProfileAsync();
   if (student == null) return NotFound();
   ```

2. **Перевірити доступ до ресурсу:**
   ```csharp
   if (course.GroupId != student.GroupId)
       return NotFound("У вас немає доступу");
   ```

### Перевірки:
- **Курси:** `course.GroupId == student.GroupId`
- **Завдання:** `assignment.Course.GroupId == student.GroupId`
- **Роботи:** `submission.StudentProfileId == student.Id`

---

## 📝 TempData Messages

```csharp
TempData["Success"] = "Робота успішно подана!";
TempData["Info"] = "Ви вже подали роботу. Можете переподати.";
TempData["Error"] = "Неможливо видалити оцінену роботу!";
```

---

## 🎯 Статуси завдань

### На сторінці Assignments:

| Іконка | Статус | Умова |
|--------|--------|-------|
| ✅ | Оцінено | MySubmission.Grade != null |
| ⏳ | Очікує перевірки | MySubmission != null && Grade == null |
| 📝 | Не подано | MySubmission == null |
| ⚠️ | Прострочено | DueDate < DateTime.Now && MySubmission == null |

---

## 🎨 Відображення дедлайну

### Кольорове кодування:

```cshtml
@if (ViewBag.IsOverdue)
{
    <span class="badge bg-danger">Прострочено</span>
}
else if (ViewBag.DaysUntilDue <= 3)
{
    <span class="badge bg-warning">Залишилось @ViewBag.DaysUntilDue днів</span>
}
else
{
    <span class="badge bg-success">До @assignment.DueDate.ToShortDateString()</span>
}
```

---

## 📊 Таблиця оцінок (CourseGrades View)

**Структура:**
```
+----------------+-------------+--------+----------+
| Завдання       | Дедлайн     | Бали   | Статус   |
+----------------+-------------+--------+----------+
| Лаб. робота 1  | 28.05.2026  | 95/100 | ✅ Оцінено|
| Лаб. робота 2  | 04.06.2026  | -      | ⏳ Очікує |
| Лаб. робота 3  | 11.06.2026  | -      | 📝 Не подано |
+----------------+-------------+--------+----------+
| Всього:        |             | 95/300 | 31.67%   |
+----------------+-------------+--------+----------+
```

---

## 📈 Статистика профілю (Index View)

**Dashboard студента:**

```
┌─────────────────────────────────────┐
│  Мої курси: 5                       │
│  Подано робіт: 18                   │
│  Оцінено робіт: 12                  │
│  Середній бал: 87.5                 │
└─────────────────────────────────────┘
```

---

## ⚠️ Валідації

### 1. Видалення роботи:
```csharp
if (submission.Grade != null)
{
    TempData["Error"] = "Неможливо видалити оцінену роботу!";
    return RedirectToAction(...);
}
```

### 2. Перевірка доступу до курсу:
```csharp
if (course == null || course.GroupId != student.GroupId)
{
    return NotFound("Курс не знайдено або немає доступу");
}
```

### 3. Перевірка доступу до роботи:
```csharp
if (submission == null || submission.StudentProfileId != student.Id)
{
    return NotFound("Робота не знайдена");
}
```

---

## 🔁 Логіка переподання роботи

### Сценарій:
1. Студент подав роботу
2. Викладач оцінив на 70/100
3. Студент хоче переподати для кращої оцінки

### Реалізація:
```csharp
if (existingSubmission != null)
{
    // Оновлюємо контент і дату
    existingSubmission.Content = submission.Content;
    existingSubmission.SubmittedAt = DateTime.Now;
    
    // Видаляємо стару оцінку
    if (existingSubmission.Grade != null)
    {
        _context.Grades.Remove(existingSubmission.Grade);
        TempData["Info"] = "Попередня оцінка видалена. Викладач має оцінити заново.";
    }
}
```

**Результат:** Викладач бачить нову роботу без оцінки

---

## 📂 Необхідні Views

```
Views/Student/
├── Index.cshtml                    (UC13: Профіль + Dashboard)
├── MyCourses.cshtml                (UC14: Список курсів)
├── CourseDetails.cshtml            (UC14: Деталі курсу)
├── Assignments.cshtml              (UC15: Список завдань + статуси)
├── AssignmentDetails.cshtml        (UC15: Деталі завдання + файли)
├── SubmitAssignment.cshtml         (UC16: Форма подання)
├── MySubmissions.cshtml            (UC16: Мої роботи)
├── SubmissionDetails.cshtml        (UC16: Деталі моєї роботи)
├── MyGrades.cshtml                 (UC17: Всі оцінки по курсах)
└── CourseGrades.cshtml             (UC17: Оцінки одного курсу)
```

---

## 🔗 Навігація між сторінками

```
Index (Dashboard)
  └─> MyCourses
       └─> CourseDetails
            ├─> Assignments
            │     └─> AssignmentDetails
            │           └─> SubmitAssignment
            │                 └─> MySubmissions
            │                       └─> SubmissionDetails
            ├─> MySubmissions
            │     └─> SubmissionDetails
            └─> CourseGrades
                  └─> SubmissionDetails

MyGrades
  └─> CourseGrades
       └─> SubmissionDetails
```

---

## 🎯 Підсумок методів

| Категорія | Кількість методів |
|-----------|-------------------|
| UC13: Профіль | 1 |
| UC14: Курси | 2 |
| UC15: Завдання | 2 |
| UC16: Подання робіт | 5 |
| UC17: Перегляд оцінок | 2 |
| Helper | 1 |
| **Всього** | **13 методів** |

---

## 💡 Корисні патерни

### 1. Перевірка доступу через GroupId:
```csharp
var course = await _context.Courses
    .FirstOrDefaultAsync(c => c.Id == courseId && c.GroupId == student.GroupId);
```

### 2. Запит з підзапитом для статусу:
```csharp
.Select(a => new
{
    Assignment = a,
    MySubmission = _context.Submissions
        .Include(s => s.Grade)
        .FirstOrDefault(...)
})
```

### 3. Групування оцінок по курсах:
```csharp
var gradesByCourse = grades
    .GroupBy(g => g.Submission!.Assignment!.Course)
    .ToList();
```

### 4. Підрахунок середнього балу:
```csharp
var averageGrade = grades.Any() 
    ? Math.Round(grades.Average(g => g.Points), 2) 
    : 0;
```

---

## 📊 Приклад використання у View (Assignments)

```cshtml
@foreach (var item in ViewBag.Assignments)
{
    <tr>
        <td>@item.Assignment.Title</td>
        <td>@item.Assignment.DueDate.ToShortDateString()</td>
        <td>
            @if (item.MySubmission?.Grade != null)
            {
                <span class="badge bg-success">
                    ✅ @item.MySubmission.Grade.Points/@item.Assignment.MaxPoints
                </span>
            }
            else if (item.MySubmission != null)
            {
                <span class="badge bg-warning">⏳ Очікує перевірки</span>
            }
            else
            {
                <span class="badge bg-secondary">📝 Не подано</span>
            }
        </td>
        <td>
            <a href="@Url.Action("AssignmentDetails", new { id = item.Assignment.Id })">
                Деталі
            </a>
            @if (item.MySubmission == null || item.MySubmission.Grade == null)
            {
                <a href="@Url.Action("SubmitAssignment", new { assignmentId = item.Assignment.Id })">
                    Подати роботу
                </a>
            }
        </td>
    </tr>
}
```

---

## 🎯 Взаємодія з TeacherController

| Дія студента | Вплив на викладача |
|--------------|-------------------|
| Подає роботу | З'являється в Submissions викладача |
| Переподає роботу | Оцінка видаляється, робота знову в Pending |
| Видаляє роботу | Робота зникає зі списку викладача |

**Важливо:** При переподанні старі оцінки видаляються автоматично!

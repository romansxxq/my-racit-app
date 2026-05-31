# TeacherController - Довідник методів

## 📊 Загальна інформація

**Роль доступу:** `[Authorize(Roles = "Teacher")]`  
**Use Cases:** UC8-UC12  
**Залежності:** `MyRacitDbContext`

---

## 🔐 Аутентифікація викладача

### `GetCurrentTeacherProfileAsync()`
**Приватний helper метод**

Отримує профіль поточного авторизованого викладача:
```csharp
var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
return await _context.TeacherProfiles
    .Include(t => t.User)
    .Include(t => t.Department)
    .FirstOrDefaultAsync(t => t.UserId == userId);
```

Використовується в **кожному** методі для перевірки доступу.

---

## 📚 UC8: ПЕРЕГЛЯД СВОЇХ КУРСІВ

### `Index()` / `MyCourses()`
- **GET** `/Teacher/Index`
- Головна сторінка викладача
- Показує всі курси, де `TeacherId == teacher.Id`
- Сортування: за датою початку (найновіші зверху)

**Дані:**
- Course → Subject
- Course → Group → Specialty
- Ім'я викладача через ViewBag

**Приклад:**
```csharp
var courses = await _context.Courses
    .Include(c => c.Subject)
    .Include(c => c.Group)
        .ThenInclude(g => g!.Specialty)
    .Where(c => c.TeacherId == teacher.Id)
    .OrderByDescending(c => c.StartDate)
    .ToListAsync();
```

---

### `CourseDetails(id)`
- **GET** `/Teacher/CourseDetails/5`
- Деталі курсу з завданнями та студентами
- Перевірка: `course.TeacherId == teacher.Id`

**ViewBag дані:**
- `Assignments` - завдання курсу (сортовані за CreatedAt)
- `Students` - студенти групи (сортовані за ім'ям)

---

## 📝 UC9: СТВОРЕННЯ ЗАВДАННЯ

### `CreateAssignment(courseId)` - GET
- **GET** `/Teacher/CreateAssignment?courseId=5`
- Форма створення завдання
- ViewBag містить Course (для відображення назви)

---

### `CreateAssignment(Assignment)` - POST
- **POST** `/Teacher/CreateAssignment`
- Створює нове завдання
- Встановлює `CreatedAt = DateTime.Now`
- Перевіряє доступ до курсу

**Редірект:** `AssignmentDetails` після успішного створення

---

### `EditAssignment(id)` - GET/POST
- **GET** `/Teacher/EditAssignment/5`
- **POST** `/Teacher/EditAssignment/5`
- Редагування існуючого завдання
- Оновлює: Title, Description, DueDate, MaxPoints

**Безпека:** Перевіряє `assignment.Course.TeacherId == teacher.Id`

---

### `AssignmentDetails(id)`
- **GET** `/Teacher/AssignmentDetails/5`
- Деталі завдання з роботами студентів
- Показує статистику виконання

**ViewBag дані:**
- `Files` - файли завдання (AssignmentFiles)
- `Links` - посилання завдання (AssignmentLinks)
- `Submissions` - подані роботи з Grade
- `TotalStudents` - кількість студентів групи
- `SubmittedCount` - кількість поданих робіт
- `GradedCount` - кількість оцінених робіт

**Приклад статистики:**
```csharp
ViewBag.TotalStudents = 25;
ViewBag.SubmittedCount = 18;  // 72%
ViewBag.GradedCount = 12;     // 48%
```

---

### `DeleteAssignment(id)`
- **POST** `/Teacher/DeleteAssignment/5`
- Видалення завдання
- **Валідація:** Неможливо видалити завдання з поданими роботами

**Редірект:** `CourseDetails` після видалення

---

## 📥 UC12: ПЕРЕГЛЯД ПОДАНИХ РОБІТ

### `Submissions(assignmentId)`
- **GET** `/Teacher/Submissions?assignmentId=5`
- Список всіх поданих робіт для завдання
- Сортування: за датою подачі (найновіші зверху)

**ViewBag:** `Assignment` - інформація про завдання

---

### `SubmissionDetails(id)`
- **GET** `/Teacher/SubmissionDetails/5`
- Детальна інформація про одну роботу
- Показує: контент роботи, студента, оцінку (якщо є)

**Дані:**
- Submission → Assignment → Course
- Submission → StudentProfile → User
- Submission → Grade

---

## ✅ UC10: ОЦІНЮВАННЯ РОБІТ

### `GradeSubmission(id)` - GET
- **GET** `/Teacher/GradeSubmission/5`
- Форма оцінювання роботи студента
- Якщо оцінка вже існує - показує її для редагування

**ViewBag:**
- `Submission` - деталі роботи
- `MaxPoints` - максимальний бал з Assignment

**Логіка:**
```csharp
var grade = submission.Grade ?? new Grade
{
    SubmissionId = submission.Id,
    StudentProfileId = submission.StudentProfileId
};
```

---

### `GradeSubmission(Grade)` - POST
- **POST** `/Teacher/GradeSubmission`
- Зберігає або оновлює оцінку

**Валідація балів:**
```csharp
if (grade.Points < 0 || grade.Points > submission.Assignment.MaxPoints)
{
    ModelState.AddModelError("Points", 
        $"Бали мають бути від 0 до {submission.Assignment.MaxPoints}");
}
```

**Логіка:**
- Якщо `submission.Grade == null` → створює нову оцінку
- Якщо існує → оновлює Points, Feedback, GradedAt

**Редірект:** `AssignmentDetails` після збереження

---

## 📊 UC11: ПЕРЕГЛЯД ОЦІНОК КУРСУ

### `Grades(courseId)`
- **GET** `/Teacher/Grades?courseId=5`
- Табличний перегляд оцінок всіх студентів курсу
- Матриця: Студенти × Завдання

**ViewBag дані:**
- `Course` - інформація про курс
- `Students` - список студентів групи
- `Assignments` - список завдань курсу
- `Grades` - всі оцінки з Submission → Assignment

**Використання у View:**
```cshtml
@foreach (var student in ViewBag.Students)
{
    @foreach (var assignment in ViewBag.Assignments)
    {
        var grade = ViewBag.Grades
            .FirstOrDefault(g => g.StudentProfileId == student.Id 
                              && g.Submission.AssignmentId == assignment.Id);
        
        <td>@(grade?.Points ?? "-")</td>
    }
}
```

---

### `StudentGrades(courseId, studentId)`
- **GET** `/Teacher/StudentGrades?courseId=5&studentId=10`
- Деталі оцінок одного студента по всіх завданнях курсу
- Показує: завдання, чи подав роботу, бал, фідбек

**ViewBag дані:**
- `Course` - курс
- `Student` - профіль студента
- `Assignments` - список об'єктів з Assignment + Submission (якщо є)

**Запит з підзапитом:**
```csharp
var assignments = await _context.Assignments
    .Where(a => a.CourseId == courseId)
    .Select(a => new
    {
        Assignment = a,
        Submission = _context.Submissions
            .Include(s => s.Grade)
            .FirstOrDefault(s => s.AssignmentId == a.Id 
                              && s.StudentProfileId == studentId)
    })
    .ToListAsync();
```

---

## 🔒 Безпека

### Перевірка доступу в кожному методі:

1. **Отримати профіль викладача:**
   ```csharp
   var teacher = await GetCurrentTeacherProfileAsync();
   if (teacher == null) return NotFound();
   ```

2. **Перевірити доступ до ресурсу:**
   ```csharp
   if (course.TeacherId != teacher.Id)
       return NotFound("У вас немає доступу");
   ```

### Перевірки:
- **Курси:** `course.TeacherId == teacher.Id`
- **Завдання:** `assignment.Course.TeacherId == teacher.Id`
- **Роботи:** `submission.Assignment.Course.TeacherId == teacher.Id`

---

## 📝 TempData Messages

```csharp
TempData["Success"] = "Завдання успішно створено!";
TempData["Error"] = "Неможливо видалити завдання з роботами!";
```

---

## 🎯 Приклади Include для навігаційних властивостей

### Курси викладача:
```csharp
.Include(c => c.Subject)
.Include(c => c.Group)
    .ThenInclude(g => g!.Specialty)
```

### Завдання з роботами:
```csharp
.Include(a => a.Course)
    .ThenInclude(c => c!.Subject)
.Include(a => a.Course)
    .ThenInclude(c => c!.Group)
```

### Роботи студентів:
```csharp
.Include(s => s.StudentProfile)
    .ThenInclude(sp => sp!.User)
.Include(s => s.Grade)
```

### Оцінки курсу:
```csharp
.Include(g => g.Submission)
    .ThenInclude(s => s!.Assignment)
```

---

## 🎨 Статистика виконання

### На сторінці AssignmentDetails:

| Показник | Значення | Формула |
|----------|----------|---------|
| Всього студентів | 25 | `StudentProfiles.Count(GroupId)` |
| Подали роботу | 18 (72%) | `Submissions.Count(AssignmentId)` |
| Оцінено | 12 (48%) | `Submissions.Count(Grade != null)` |
| Не подали | 7 (28%) | TotalStudents - SubmittedCount |

---

## 📊 Таблиця оцінок (Grades View)

**Структура:**
```
+-------------+------------+------------+------------+
| Студент     | Завдання 1 | Завдання 2 | Завдання 3 |
+-------------+------------+------------+------------+
| Іванов І.І. | 95/100     | -          | 87/100     |
| Петров П.П. | 78/100     | 90/100     | -          |
+-------------+------------+------------+------------+
```

**Легенда:**
- `95/100` - оцінений
- `-` - не подав роботу
- `Подано` - подав, але не оцінено

---

## ⚠️ Валідації

### 1. Видалення завдання:
```csharp
var hasSubmissions = await _context.Submissions.AnyAsync(s => s.AssignmentId == id);
if (hasSubmissions)
{
    TempData["Error"] = "Неможливо видалити завдання з роботами!";
    return RedirectToAction(...);
}
```

### 2. Валідація балів:
```csharp
if (grade.Points < 0 || grade.Points > maxPoints)
{
    ModelState.AddModelError("Points", "Бали поза межами!");
}
```

### 3. Перевірка доступу до курсу:
```csharp
if (course == null || course.TeacherId != teacher.Id)
{
    return NotFound("Курс не знайдено або немає доступу");
}
```

---

## 📂 Необхідні Views

```
Views/Teacher/
├── Index.cshtml                    (UC8: Мої курси)
├── CourseDetails.cshtml            (UC8: Деталі курсу)
├── CreateAssignment.cshtml         (UC9: Форма створення)
├── EditAssignment.cshtml           (UC9: Форма редагування)
├── AssignmentDetails.cshtml        (UC9: Деталі + роботи)
├── Submissions.cshtml              (UC12: Список робіт)
├── SubmissionDetails.cshtml        (UC12: Деталі роботи)
├── GradeSubmission.cshtml          (UC10: Форма оцінювання)
├── Grades.cshtml                   (UC11: Таблиця оцінок)
└── StudentGrades.cshtml            (UC11: Оцінки студента)
```

---

## 🔗 Навігація між сторінками

```
Index (Мої курси)
  └─> CourseDetails
       ├─> CreateAssignment
       │     └─> AssignmentDetails
       │           ├─> Submissions
       │           │     └─> SubmissionDetails
       │           │           └─> GradeSubmission
       │           └─> GradeSubmission (швидкий доступ)
       └─> Grades
             └─> StudentGrades
```

---

## 🎯 Підсумок методів

| Категорія | Кількість методів |
|-----------|-------------------|
| UC8: Курси | 2 |
| UC9: Завдання | 6 |
| UC10: Оцінювання | 2 |
| UC11: Перегляд оцінок | 2 |
| UC12: Роботи студентів | 2 |
| Helper | 1 |
| **Всього** | **15 методів** |

---

## 💡 Корисні патерни

### 1. Перевірка доступу в одному місці:
```csharp
var teacher = await GetCurrentTeacherProfileAsync();
if (teacher == null) return NotFound("Профіль не знайдено");
```

### 2. Оновлення існуючої або створення нової оцінки:
```csharp
if (submission.Grade == null)
{
    _context.Grades.Add(grade);
}
else
{
    submission.Grade.Points = grade.Points;
    submission.Grade.Feedback = grade.Feedback;
}
await _context.SaveChangesAsync();
```

### 3. Запит з підзапитом (для Grades):
```csharp
.Select(a => new
{
    Assignment = a,
    Submission = _context.Submissions.FirstOrDefault(...)
})
```

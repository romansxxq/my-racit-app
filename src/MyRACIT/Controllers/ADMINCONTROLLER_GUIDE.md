# AdminController - Довідник методів

## 📊 Загальна інформація

**Роль доступу:** `[Authorize(Roles = "Admin")]`  
**Use Cases:** UC1-UC7  
**Залежності:** `IUserService`, `MyRacitDbContext`

---

## 🏠 DASHBOARD

### `Index()`
- **GET** `/Admin/Index`
- Головна сторінка адміна з статистикою
- Відображає: кількість студентів, викладачів, курсів, груп

---

## 🏢 UC1: КЕРУВАННЯ КАФЕДРАМИ

| Метод | HTTP | Маршрут | Опис |
|-------|------|---------|------|
| `Departments()` | GET | `/Admin/Departments` | Список всіх кафедр |
| `CreateDepartment()` | GET | `/Admin/CreateDepartment` | Форма створення |
| `CreateDepartment(Department)` | POST | `/Admin/CreateDepartment` | Створити кафедру |
| `EditDepartment(id)` | GET | `/Admin/EditDepartment/5` | Форма редагування |
| `EditDepartment(id, Department)` | POST | `/Admin/EditDepartment/5` | Оновити кафедру |
| `DeleteDepartment(id)` | POST | `/Admin/DeleteDepartment/5` | Видалити кафедру |

**Валідація видалення:** Неможливо видалити кафедру з прив'язаними викладачами

---

## 🎓 UC2: КЕРУВАННЯ СПЕЦІАЛЬНОСТЯМИ

| Метод | HTTP | Маршрут | Опис |
|-------|------|---------|------|
| `Specialties()` | GET | `/Admin/Specialties` | Список спеціальностей |
| `CreateSpecialty()` | GET | `/Admin/CreateSpecialty` | Форма створення |
| `CreateSpecialty(Specialty)` | POST | `/Admin/CreateSpecialty` | Створити спеціальність |
| `EditSpecialty(id)` | GET | `/Admin/EditSpecialty/5` | Форма редагування |
| `EditSpecialty(id, Specialty)` | POST | `/Admin/EditSpecialty/5` | Оновити спеціальність |
| `DeleteSpecialty(id)` | POST | `/Admin/DeleteSpecialty/5` | Видалити спеціальність |

**Валідація видалення:** Неможливо видалити спеціальність з прив'язаними групами

---

## 👥 UC3: КЕРУВАННЯ ГРУПАМИ

| Метод | HTTP | Маршрут | Опис |
|-------|------|---------|------|
| `Groups()` | GET | `/Admin/Groups` | Список груп (з спеціальностями) |
| `CreateGroup()` | GET | `/Admin/CreateGroup` | Форма створення + dropdown спеціальностей |
| `CreateGroup(Group)` | POST | `/Admin/CreateGroup` | Створити групу |
| `EditGroup(id)` | GET | `/Admin/EditGroup/5` | Форма редагування |
| `EditGroup(id, Group)` | POST | `/Admin/EditGroup/5` | Оновити групу |
| `DeleteGroup(id)` | POST | `/Admin/DeleteGroup/5` | Видалити групу |

**Валідація:**
- Перевірка унікальності назви групи
- Неможливо видалити групу зі студентами

---

## 📚 UC4: КЕРУВАННЯ ДИСЦИПЛІНАМИ

| Метод | HTTP | Маршрут | Опис |
|-------|------|---------|------|
| `Subjects()` | GET | `/Admin/Subjects` | Список дисциплін |
| `CreateSubject()` | GET | `/Admin/CreateSubject` | Форма створення |
| `CreateSubject(Subject)` | POST | `/Admin/CreateSubject` | Створити дисципліну |
| `EditSubject(id)` | GET | `/Admin/EditSubject/5` | Форма редагування |
| `EditSubject(id, Subject)` | POST | `/Admin/EditSubject/5` | Оновити дисципліну |
| `DeleteSubject(id)` | POST | `/Admin/DeleteSubject/5` | Видалити дисципліну |

**Валідація видалення:** Неможливо видалити дисципліну, яка використовується в курсах

---

## 🎒 UC5: КЕРУВАННЯ СТУДЕНТАМИ

| Метод | HTTP | Маршрут | Опис |
|-------|------|---------|------|
| `Students()` | GET | `/Admin/Students` | Список студентів (User + Group + Specialty) |
| `CreateStudent()` | GET | `/Admin/CreateStudent` | Форма створення + dropdown груп |
| `CreateStudent(CreateStudentDto)` | POST | `/Admin/CreateStudent` | Створити студента через UserService |
| `StudentDetails(id)` | GET | `/Admin/StudentDetails/5` | Деталі студента + його курси |

**Логіка створення:**
```csharp
await _userService.CreateStudentAsync(name, email, password, groupId);
// Автоматично створює User + StudentProfile
```

**Дані деталей:**
- Профіль студента (ПІБ, email, група, спеціальність)
- Список курсів групи (через GroupId)

---

## 👨‍🏫 UC6: КЕРУВАННЯ ВИКЛАДАЧАМИ

| Метод | HTTP | Маршрут | Опис |
|-------|------|---------|------|
| `Teachers()` | GET | `/Admin/Teachers` | Список викладачів (User + Department) |
| `CreateTeacher()` | GET | `/Admin/CreateTeacher` | Форма створення + dropdown кафедр |
| `CreateTeacher(CreateTeacherDto)` | POST | `/Admin/CreateTeacher` | Створити викладача через UserService |
| `TeacherDetails(id)` | GET | `/Admin/TeacherDetails/5` | Деталі викладача + його курси |

**Логіка створення:**
```csharp
await _userService.CreateTeacherAsync(name, email, password, departmentId);
// Автоматично створює User + TeacherProfile
```

**Дані деталей:**
- Профіль викладача (ПІБ, email, кафедра)
- Список курсів викладача (де TeacherId == id)

---

## 📖 UC7: КЕРУВАННЯ КУРСАМИ

| Метод | HTTP | Маршрут | Опис |
|-------|------|---------|------|
| `Courses()` | GET | `/Admin/Courses` | Список курсів (Subject + Teacher + Group) |
| `CreateCourse()` | GET | `/Admin/CreateCourse` | Форма + 3 dropdown (предмети, викладачі, групи) |
| `CreateCourse(Course)` | POST | `/Admin/CreateCourse` | Створити курс |
| `CourseDetails(id)` | GET | `/Admin/CourseDetails/5` | Деталі курсу + студенти + завдання |
| `DeleteCourse(id)` | POST | `/Admin/DeleteCourse/5` | Видалити курс |

**Дані створення:**
- Subject (дисципліна)
- Teacher (викладач)
- Group (група)
- StartDate, EndDate (дати семестру)

**Дані деталей:**
- Інформація про курс
- Студенти групи (автоматично записані на курс)
- Завдання курсу

**Валідація видалення:** Неможливо видалити курс з завданнями

---

## 🛠️ HELPER METHODS

### `LoadCourseViewBags()`
**Приватний метод**  
Завантажує списки для dropdown при створенні курсу:
- Subjects
- Teachers (з User та Department)
- Groups (зі Specialty)

---

## 🔒 Безпека

Всі методи контролера захищені атрибутом:
```csharp
[Authorize(Roles = "Admin")]
```

Доступ мають **тільки** користувачі з роллю `UserRole.Admin`.

---

## 📝 TempData Messages

Контролер використовує `TempData` для повідомлень:

```csharp
TempData["Success"] = "Операцію виконано успішно!";
TempData["Error"] = "Помилка: опис помилки";
```

Відображати в Layout:
```cshtml
@if (TempData["Success"] != null)
{
    <div class="alert alert-success">@TempData["Success"]</div>
}
@if (TempData["Error"] != null)
{
    <div class="alert alert-danger">@TempData["Error"]</div>
}
```

---

## 📊 Підрахунок записів (Dashboard)

```csharp
ViewBag.TotalStudents = await _context.StudentProfiles.CountAsync();
ViewBag.TotalTeachers = await _context.TeacherProfiles.CountAsync();
ViewBag.TotalCourses = await _context.Courses.CountAsync();
ViewBag.TotalGroups = await _context.Groups.CountAsync();
```

---

## 🔗 Навігаційні властивості

Контролер використовує `.Include()` для завантаження пов'язаних даних:

```csharp
// Приклад для студентів
var students = await _context.StudentProfiles
    .Include(s => s.User)              // Завантажити User
    .Include(s => s.Group)             // Завантажити Group
        .ThenInclude(g => g!.Specialty)  // Завантажити Specialty
    .ToListAsync();
```

---

## ✅ Валідація перед видаленням

Перед видаленням контролер перевіряє наявність залежних записів:

```csharp
// Приклад для кафедри
var hasTeachers = await _context.TeacherProfiles.AnyAsync(t => t.DepartmentId == id);
if (hasTeachers)
{
    TempData["Error"] = "Неможливо видалити кафедру, до якої прив'язані викладачі!";
    return RedirectToAction(nameof(Departments));
}
```

Це запобігає порушенню цілісності даних.

---

## 🎯 Приклад використання ViewBag

```csharp
// В контролері:
ViewBag.Groups = await _context.Groups.ToListAsync();

// У View:
<select asp-for="GroupId" class="form-control">
    @foreach (var group in ViewBag.Groups)
    {
        <option value="@group.Id">@group.Name</option>
    }
</select>
```

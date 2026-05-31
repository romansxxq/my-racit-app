# MyRACIT 🎓

**My**RACIT — веб-система управління навчальним процесом закладу передвищої освіти **РФКІТ** (Рівненський фаховий коледж інформаційних технологій).

[![ASP.NET Core](https://img.shields.io/badge/ASP.NET%20Core-11.0-blue)](https://dotnet.microsoft.com/)
[![EF Core](https://img.shields.io/badge/EF%20Core-10.0-purple)](https://docs.microsoft.com/ef/)
[![SQL Server](https://img.shields.io/badge/SQL%20Server-Express-red)](https://www.microsoft.com/sql-server)
[![xUnit](https://img.shields.io/badge/Tests-25%20passed-brightgreen)](https://xunit.net/)

---

## 📋 Опис проєкту

**Предметна область:** Адміністрування та організація навчання у закладі передвищої освіти РФКІТ. Система координує взаємодію між адміністрацією, викладачами та студентами — від реєстрації користувачів і розподілу по групах до створення завдань, подання робіт і виставлення оцінок.

### 🎯 Мета
Автоматизувати рутинні процеси обліку студентів, формування академічних груп, запису на курси та оцінювання — через єдину захищену веб-платформу з розмежуванням ролей.

---

## 👥 Актори системи

### 🔴 Адміністратор
Керує глобальними довідниками, реєструє нові групи та дисципліни, створює облікові записи для студентів і викладачів.

**Функції:**
- Управління кафедрами, спеціальностями та групами
- Реєстрація користувачів (студентів і викладачів)
- Створення курсів та призначення викладачів
- Деактивація облікових записів

### 🔵 Викладач
Веде закріплені курси, переглядає подані роботи, виставляє оцінки.

**Функції:**
- Перегляд своїх курсів та списків студентів
- Створення завдань (текст, файли, посилання, дедлайн, макс. бал)
- Перегляд та оцінювання поданих робіт студентів

### 🟢 Студент
Переглядає свої курси, подає виконані роботи, стежить за оцінками.

**Функції:**
- Перегляд активних курсів та завдань
- Подання виконаних робіт (текст або файл)
- Перегляд оцінок та коментарів викладача
- Редагування особистого профілю

---

## 🛠 Технологічний стек

| Шар | Технологія |
|-----|-----------|
| Backend | ASP.NET Core MVC (.NET 11.0-preview) |
| ORM | Entity Framework Core 10.0 |
| База даних | Microsoft SQL Server Express |
| Автентифікація | Cookie Authentication (власна реалізація) |
| Frontend | Razor Views, Bootstrap 5, jQuery |
| Тестування | xUnit 2.9.3, Moq, EF Core InMemory |
| CI | GitHub Actions |

---

## 🗂 Структура проєкту

```
my-racit-app/
├── src/
│   └── MyRACIT/
│       ├── Controllers/            # MVC контролери
│       │   ├── AdminController.cs  # Управління довідниками, реєстрація
│       │   ├── AuthController.cs   # Вхід / вихід
│       │   ├── StudentController.cs
│       │   └── TeacherController.cs
│       ├── Data/
│       │   ├── MyRacitDbContext.cs
│       │   └── DataSeeder.cs       # Початкове наповнення тестовими даними
│       ├── Migrations/             # EF Core міграції
│       ├── Models/
│       │   ├── Entities/           # Assignment, Course, Grade, Group, ...
│       │   ├── DTOs/               # UserDtos
│       │   └── Exceptions/         # RacitException ієрархія
│       ├── Services/
│       │   ├── UserService.cs      # Бізнес-логіка користувачів
│       │   ├── FileStorageService.cs
│       │   └── Interfaces/
│       ├── Views/                  # Razor Views (Admin/Auth/Student/Teacher)
│       └── wwwroot/
├── tests/
│   └── MyRACIT.Tests/
│       ├── Helpers/DbFactory.cs
│       ├── UserService_AuthenticateTests.cs    # 4 тести
│       ├── UserService_ChangePasswordTests.cs  # 3 тести
│       ├── UserService_CreateAdminTests.cs     # 3 тести
│       ├── UserService_CreateStudentTests.cs   # 3 тести
│       ├── UserService_CreateTeacherTests.cs   # 3 тести
│       ├── UserService_EmailExistsTests.cs     # 2 тести
│       ├── UserService_GetUserTests.cs         # 4 тести
│       └── UserService_UpdateProfileTests.cs   # 4 тести
├── UML-diagrams.md
├── USE-CASES.md
├── ARCHITECTURE.md
└── README.md
```

---

## 🚀 Швидкий старт

### Вимоги
- [.NET 11.0 SDK (preview)](https://dotnet.microsoft.com/download)
- Microsoft SQL Server Express
- Visual Studio 2022+ або VS Code

### Установка

1. **Клонувати репозиторій**
   ```bash
   git clone https://github.com/yourusername/my-racit-app.git
   cd my-racit-app
   ```

2. **Налаштувати рядок підключення**

   Відредагуйте `src/MyRACIT/appsettings.json`:
   ```json
   "ConnectionStrings": {
     "DefaultConnection": "Server=YOUR_SERVER\\SQLEXPRESS;Database=myracitdb;Trusted_Connection=True;TrustServerCertificate=True;"
   }
   ```

3. **Застосувати міграції**
   ```bash
   cd src/MyRACIT
   dotnet ef database update
   ```

4. **Запустити проєкт**
   ```bash
   dotnet run
   ```
   Застосунок підніметься на `http://localhost:5240`.  
   При першому запуску `DataSeeder` автоматично наповнить базу тестовими даними.

---

## � Архітектура та патерни

### MVC + Service Layer
Проєкт реалізує класичний **MVC** з виділеним сервісним шаром:
- **Controllers** — приймають HTTP-запити, делегують логіку сервісам, передають дані до View
- **Services** (`IUserService`, `IFileStorageService`) — вся бізнес-логіка, ізольована від контролерів
- **Entities** — POCO-класи, відображені на таблиці через EF Core

### Патерни
| Патерн | Де використовується |
|--------|-------------------|
| Repository (через EF Core DbContext) | `MyRacitDbContext` — єдина точка доступу до БД |
| Dependency Injection | Всі сервіси реєструються у `Program.cs`, впроваджуються через конструктор |
| DTO | `UserDtos` — передача даних між шарами без прямого використання Entity |
| Custom Exception Hierarchy | `RacitException` → `UserAlreadyExistsException` / `GroupNotFoundException` / `DepartmentNotFoundException` |
| Cookie Authentication | Власна реалізація входу/виходу через `CookieAuthenticationDefaults` |

### Безпека
- Паролі хешуються через `BCrypt` (без збереження plain-text)
- Захист маршрутів через `[Authorize]` / `[Authorize(Policy = ...)]`
- Cookie: `HttpOnly`, `SameSite=Lax`, TTL 8 годин

---

## 📊 Модель даних

| Сутність | Опис |
|----------|------|
| `User` | Базовий обліковий запис (роль: Admin/Teacher/Student, хеш пароля) |
| `StudentProfile` | Профіль студента → `Group` |
| `TeacherProfile` | Профіль викладача → `Department` |
| `Department` | Кафедра |
| `Specialty` | Спеціальність → `Department` |
| `Group` | Академічна група → `Specialty` |
| `Subject` | Навчальна дисципліна → `Department` |
| `Course` | Subject + TeacherProfile + Group + Semester |
| `Assignment` | Завдання → `Course` (текст, файли, посилання, дедлайн, MaxGrade) |
| `Submission` | Відповідь студента → `Assignment` (текст або файл) |
| `Grade` | Оцінка → `Submission` (числовий бал + коментар) |

Детальні схеми: [ARCHITECTURE.md](ARCHITECTURE.md) · [UML-diagrams.md](UML-diagrams.md)

---

## 🔐 Матриця доступу

| Функція | Admin | Teacher | Student |
|---------|:-----:|:-------:|:-------:|
| Управління довідниками | ✅ | ❌ | ❌ |
| Реєстрація користувачів | ✅ | ❌ | ❌ |
| Створення курсів | ✅ | ❌ | ❌ |
| Створення завдань | ❌ | ✅ (свої курси) | ❌ |
| Оцінювання робіт | ❌ | ✅ (свої курси) | ❌ |
| Подання робіт | ❌ | ❌ | ✅ (свої курси) |
| Перегляд оцінок | ✅ | ✅ (свої курси) | ✅ (лише своїх) |

---

## 🧪 Тестування

```bash
# Запустити всі тести
dotnet test tests/MyRACIT.Tests

# Зібрати покриття
dotnet test tests/MyRACIT.Tests /p:CollectCoverage=true /p:CoverletOutputFormat=lcov

# HTML-звіт (потрібен reportgenerator)
dotnet reportgenerator -reports:"coverage.info" -targetdir:"coverage-report" -reporttypes:Html
```

### Поточний стан тестів
- **25 unit-тестів**, всі проходять
- Покриття `UserService`: **~88%**
- Тести перевіряють: автентифікацію, реєстрацію (Admin/Teacher/Student), зміну пароля, оновлення профілю, кастомні виключення

---

## 📄 Документація

| Файл | Зміст |
|------|-------|
| [ARCHITECTURE.md](ARCHITECTURE.md) | Технічна архітектура, схема БД, сервіси |
| [UML-diagrams.md](UML-diagrams.md) | Use Case, Class, ER, Sequence, Activity, Component діаграми |
| [USE-CASES.md](USE-CASES.md) | 19 детальних сценаріїв використання з альтернативними потоками |
| [PROGRESS_STATUS.md](PROGRESS_STATUS.md) | Поточний статус реалізації функціоналу |

---

## 📝 EF Core — корисні команди

```bash
# Нова міграція
dotnet ef migrations add MigrationName --project src/MyRACIT

# Застосувати до БД
dotnet ef database update --project src/MyRACIT

# Відкотити
dotnet ef database update PreviousMigrationName --project src/MyRACIT
```

### Міграції проєкту
| Міграція | Зміни |
|----------|-------|
| `20260527_InitialMigration` | Початкова структура БД |
| `20260528_UpdateGradeAndAddAssignmentResources` | Ресурси завдань, оновлення Grade |
| `20260528_AddSubmissionGradeNavigation` | Навігаційна властивість Submission→Grade |
| `20260529_FixGradeSubmissionRelationship` | Виправлення зв'язку Grade↔Submission |
| `20260529_AddSubmissionContent` | Поле Content у Submission |

---

## 👨‍💻 Автор

- **Матвійчук Роман**
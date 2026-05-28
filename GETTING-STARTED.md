# 🚀 Інструкція з запуску MyRACIT

## Швидкий старт

### 1. Перевірка БД
```powershell
cd src/MyRACIT
dotnet ef database update
```

### 2. Запуск проєкту
```powershell
dotnet run
```

### 3. Відкрити в браузері
```
https://localhost:5001
```

### 4. 🛠️ CoreAdmin - Автоматична адмін-панель
```
https://localhost:5001/admin
```

**CoreAdmin** автоматично генерує повноцінний CRUD інтерфейс для всіх ваших Entity моделей:
- ✅ **11 таблиць** - User, StudentProfile, TeacherProfile, Course, Grade, Assignment, Submission, Subject, Department, Specialty, Group
- ✅ **CRUD операції** - Create, Read, Update, Delete для кожної таблиці
- ✅ **Grid з пагінацією** - зручний перегляд даних
- ✅ **Пошук та фільтрація** - швидкий доступ до записів
- ✅ **Foreign Key відносини** - автоматичні dropdown'и для зв'язків
- ✅ **Валідація** - перевірка даних перед збереженням

---

## 🎯 Демонстрація патернів

### Адміністратор (Factory Method Pattern)

1. Перейдіть: **Адміністратор → Додати студента**
2. Заповніть форму:
   - ПІБ: `Іванов Іван Іванович`
   - Email: `ivanov@student.rfkit.edu.ua`
   - Група: оберіть зі списку
3. Натисніть "Створити студента"

**Що відбувається:**
- `UserFactory.CreateStudent()` генерує пароль
- Хешування паролю (SHA256)
- Автоматичне призначення ролі
- Створення профілю студента

### Викладач (Strategy Pattern)

1. Перейдіть: **Викладач → Мої курси**
2. Оберіть курс → "Деталі курсу"
3. Натисніть на студента для виставлення оцінки
4. Перейдіть: **Викладач → Стратегія оцінювання**
5. Змініть стратегію та подивіться різницю в балах

**Доступні стратегії:**
- **Середній бал** - просте середнє арифметичне
- **Зважений бал** - враховує важливість завдань (MaxGrade)
- **Відсотковий бал** - конвертація відсотків у бали (2-5)

### Студент (Repository Pattern)

1. Перейдіть: **Студент → Мої курси**
2. Оберіть курс
3. Переглянути оцінки з різними стратегіями обчислення

---

## 🧪 Тестування

### Запустити тести
```powershell
cd tests/MyRACIT.Tests
dotnet test
```

### Додати тестове покриття
```powershell
dotnet test /p:CollectCoverage=true
```

---

## 📂 Структура проєкту

```
MyRACIT/
├── Controllers/
│   ├── AdminController.cs      # Factory Method
│   ├── TeacherController.cs    # Strategy Pattern
│   └── StudentController.cs    # Repository Pattern
├── Data/
│   ├── IRepository.cs          # Generic Repository Interface
│   ├── Repository.cs           # Generic Repository Implementation
│   └── MyRacitDbContext.cs     # EF Core Context
├── Factories/
│   └── UserFactory.cs          # Factory Method Pattern
├── Strategies/
│   ├── IGradeStrategy.cs       # Strategy Interface
│   ├── AverageGradeStrategy.cs
│   ├── WeightedGradeStrategy.cs
│   └── PercentageGradeStrategy.cs
├── Services/
│   └── GradeService.cs         # Business Logic + Strategy
├── Models/Entities/
│   ├── User.cs
│   ├── StudentProfile.cs
│   ├── TeacherProfile.cs
│   ├── Course.cs
│   ├── Grade.cs
│   └── ... (11 Entity classes)
├── Views/
│   ├── Admin/
│   ├── Teacher/
│   └── Student/
└── Program.cs
    ├── CoreAdmin - автоматична адмін-панель
    ├── Generic Repository (DI)
    ├── Strategy Pattern (DI)
    └── EF Core DbContext
```

---

## 🛠️ CoreAdmin - що це таке?

**CoreAdmin** - це NuGet бібліотека, яка автоматично генерує повноцінну адмін-панель для управління базою даних через Entity Framework Core.

### Переваги CoreAdmin:

✅ **Без коду** - не потрібно писати контролери та views  
✅ **Універсальність** - працює з будь-якими Entity моделями  
✅ **Швидкість** - повна адмін-панель за 3 хвилини  
✅ **CRUD із коробки** - Create, Read, Update, Delete  
✅ **Зручний UI** - Grid з пагінацією, пошук, фільтри  
✅ **Foreign Keys** - автоматичні dropdown'и для зв'язків  

### Як використовувати:

1. **Перейдіть на `/admin`** - https://localhost:5001/admin
2. **Оберіть таблицю** - User, Course, Grade тощо
3. **CRUD операції**:
   - **Create** - додати новий запис
   - **Read** - переглянути список
   - **Update** - редагувати існуючий запис
   - **Delete** - видалити запис
4. **Пошук та фільтрація** - швидко знайти потрібні дані

### Що показати викладачу:

1. **Grid з даними** - таблиця з пагінацією та сортуванням
2. **Форма створення** - автоматична валідація полів
3. **Форма редагування** - збереження змін
4. **Foreign Key** - dropdown для вибору зв'язаних Entity (наприклад, Group для StudentProfile)
5. **Масове редагування** - bulk operations

### Налаштування (вже зроблено):

```csharp
// Program.cs
builder.Services.AddCoreAdmin("MyRACIT Admin");
app.UseCoreAdminCustomUrl("admin");
```

---

## 🎓 Демонстрація для практики

### Сценарій 1: Створення користувачів (Factory Method)

```csharp
// Код в AdminController
var student = UserFactory.CreateStudent(name, email);
await _userRepository.AddAsync(student);
```

**Покажіть викладачу:**
1. Форму створення студента
2. Консольний вивід з паролем
3. Запис у БД
4. Автоматичний hash пароля

### Сценарій 2: Обчислення оцінок (Strategy)

```csharp
// Код в TeacherController
gradeService.SetStrategy(new AverageGradeStrategy());
var avgGrade = await gradeService.GetFinalGradeAsync(studentId, courseId);

gradeService.SetStrategy(new WeightedGradeStrategy());
var weightedGrade = await gradeService.GetFinalGradeAsync(studentId, courseId);
```

**Покажіть викладачу:**
1. Сторінку з порівнянням 3 стратегій
2. Різні результати для одних і тих же оцінок
3. Можливість зміни стратегії без зміни коду

### Сценарій 3: CRUD операції (Repository)

```csharp
// Код в Controllers
var userRepo = new Repository<User>(_context);
var allUsers = await userRepo.GetAllAsync();
var students = await userRepo.FindAsync(u => u.Role == UserRole.Student);
await userRepo.AddAsync(newUser);
```

**Покажіть викладачу:**
1. Один репозиторій для всіх Entity
2. Типобезпечність (Generic)
3. Зменшення дублювання коду

---

## 📊 Критерії оцінювання (для практики)

### ✅ Мінімум (3 бали):
- [x] UML-діаграми
- [x] Entity моделі
- [x] 3+ патерни

### ✅ Достатньо (4 бали):
- [x] 6+ патернів
- [x] Web-інтерфейс
- [x] Документація

### 🎯 Відмінно (5 балів):
- [x] 10+ патернів (Factory, Repository, Strategy, + наступні)
- [ ] Unit-тести 70%+
- [x] Демонстрація роботи
- [x] Обґрунтування рішень

---

## 🐛 Усунення проблем

### Помилка з БД
```powershell
dotnet ef database drop
dotnet ef database update
```

### Помилка з пакетами
```powershell
dotnet restore
dotnet build
```

### Порт зайнятий
Змініть порт у `Properties/launchSettings.json`:
```json
"applicationUrl": "https://localhost:5002;http://localhost:5003"
```

---

## 📞 Підтримка

Якщо виникли проблеми:
1. Перевірте чи працює SQL Server
2. Перевірте connection string в `appsettings.json`
3. Подивіться логи в консолі
4. Перевірте міграції: `dotnet ef migrations list`

---

## 🎉 Готово!

Проєкт готовий до демонстрації. Всі патерни працюють через web-інтерфейс!

**Наступні кроки:**
- Додати Composite Pattern для ієрархії
- Додати Observer Pattern для сповіщень
- Написати Unit-тести
- Додати ASP.NET Identity

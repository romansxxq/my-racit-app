# MyRACIT 🎓

**My**RACIT - Інформаційна система управління навчальним процесом закладу передвищої освіти **РФКІТ** (Рівненський фаховий коледж інформаційних технологій).

[![ASP.NET Core](https://img.shields.io/badge/ASP.NET%20Core-11.0-blue)](https://dotnet.microsoft.com/)
[![Entity Framework](https://img.shields.io/badge/EF%20Core-Latest-purple)](https://docs.microsoft.com/ef/)
[![SQL Server](https://img.shields.io/badge/SQL%20Server-2019+-red)](https://www.microsoft.com/sql-server)

---

## 📋 Опис проєкту

MyRACIT - це веб-платформа, яка автоматизує процеси обліку студентів, формування академічних груп та запису на дисципліни в закладі передвищої освіти. Система забезпечує зручну взаємодію між адміністрацією, викладачами та студентами.

### 🎯 Мета розробки
Створити веб-платформу, яка:
- Автоматизує рутинні процеси обліку студентів
- Забезпечує формування академічних груп
- Організовує запис на дисципліни
- Надає єдиний механізм авторизації для всіх користувачів
- Спрощує виставлення оцінок та перегляд успішності

---

## 👥 Актори системи

### 🔴 Адміністратор
Керує глобальними довідниками, реєструє нові групи та дисципліни, створює облікові записи для студентів та викладачів.

**Функції:**
- Управління кафедрами та спеціальностями
- Створення та редагування груп
- Реєстрація користувачів (студентів і викладачів)
- Створення курсів та призначення викладачів
- Повний доступ до всіх даних системи

### 🔵 Викладач
Веде закріплені за ним поточні курси, переглядає списки студентів, виставляє оцінки.

**Функції:**
- Перегляд закріплених курсів
- Створення завдань для курсів
- Перегляд поданих робіт студентів
- Виставлення оцінок
- Надання зворотного зв'язку студентам

### 🟢 Студент
Заповнює свій профіль, переглядає курси, на які він записаний, та бачить свої оцінки.

**Функції:**
- Перегляд своїх курсів
- Перегляд завдань та дедлайнів
- Подання виконаних робіт
- Перегляд оцінок та коментарів викладачів
- Редагування особистого профілю

---

## 🛠 Технологічний стек

- **Backend:** ASP.NET Core MVC (.NET 11.0)
- **ORM:** Entity Framework Core
- **Database:** Microsoft SQL Server (LocalDB для розробки)
- **Admin Panel:** CoreAdmin
- **Frontend:** Razor Views, Bootstrap 5, jQuery
- **Authentication:** ASP.NET Core Identity (планується)

---

## 📚 Документація

### UML-діаграми
📄 [UML-diagrams.md](UML-diagrams.md) - Повний набір UML-діаграм:
- **Use Case діаграма** - Актори та сценарії використання
- **Class діаграма** - Структура класів системи
- **ER-діаграма** - Зв'язки між сутностями бази даних
- **Sequence діаграма** - Процес виставлення оцінки
- **Activity діаграма** - Реєстрація студента на курс
- **Component діаграма** - Архітектура системи

### Сценарії використання
📄 [USE-CASES.md](USE-CASES.md) - Детальний опис всіх Use Cases:
- 19 детально описаних сценаріїв використання
- Основні та альтернативні потоки
- Матриця відповідальності (RACI)
- Пріоритети реалізації по фазам

### Технічна документація
📄 [ARCHITECTURE.md](ARCHITECTURE.md) - Технічна архітектура проєкту:
- Структура бази даних з SQL-схемами
- Бізнес-логіка та сервіси
- Правила безпеки та авторизації
- Оптимізація продуктивності
- Patterns та best practices
- API Endpoints
- Deployment інструкції

---

## 🗂 Структура проєкту

```
my-racit-app/
├── src/
│   └── MyRACIT/                    # Головний проєкт
│       ├── Controllers/            # MVC контролери
│       │   └── HomeController.cs
│       ├── Data/                   # DbContext
│       │   └── MyRacitDbContext.cs
│       ├── Migrations/             # EF Core міграції
│       ├── Models/                 # Моделі даних
│       │   ├── ErrorViewModel.cs
│       │   └── Entities/           # Entity класи
│       │       ├── User.cs
│       │       ├── StudentProfile.cs
│       │       ├── TeacherProfile.cs
│       │       ├── Department.cs
│       │       ├── Specialty.cs
│       │       ├── Group.cs
│       │       ├── Subject.cs
│       │       ├── Course.cs
│       │       ├── Assignment.cs
│       │       ├── Submission.cs
│       │       ├── Grade.cs
│       │       └── UserRole.cs
│       ├── Views/                  # Razor Views
│       │   ├── Home/
│       │   └── Shared/
│       ├── wwwroot/                # Статичні файли
│       │   ├── css/
│       │   ├── js/
│       │   └── lib/
│       ├── appsettings.json
│       ├── Program.cs
│       └── MyRACIT.csproj
├── tests/
│   └── MyRACIT.Tests/              # Unit тести
├── UML-diagrams.md                 # UML-діаграми
├── USE-CASES.md                    # Сценарії використання
├── ARCHITECTURE.md                 # Технічна документація
└── README.md                       # Цей файл
```

---

## 🚀 Швидкий старт

### Вимоги
- [.NET 11.0 SDK](https://dotnet.microsoft.com/download)
- [SQL Server](https://www.microsoft.com/sql-server) або SQL Server LocalDB
- [Visual Studio 2024](https://visualstudio.microsoft.com/) або [VS Code](https://code.visualstudio.com/)

### Установка

1. **Клонувати репозиторій**
   ```bash
   git clone https://github.com/yourusername/my-racit-app.git
   cd my-racit-app
   ```

2. **Відновити пакети**
   ```bash
   cd src/MyRACIT
   dotnet restore
   ```

3. **Оновити connection string**
   
   Відредагуйте `appsettings.json`:
   ```json
   "ConnectionStrings": {
     "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=MyRacitDb;Trusted_Connection=True;"
   }
   ```

4. **Застосувати міграції**
   ```bash
   dotnet ef database update
   ```

5. **Запустити проєкт**
   ```bash
   dotnet run
   ```

6. **Відкрити в браузері**
   ```
   https://localhost:5001
   ```

---

## 📊 Модель даних

### Основні сутності

| Сутність | Опис |
|----------|------|
| **User** | Базова інформація про користувача (Admin/Teacher/Student) |
| **StudentProfile** | Розширений профіль студента з прив'язкою до групи |
| **TeacherProfile** | Розширений профіль викладача з прив'язкою до кафедри |
| **Department** | Кафедри закладу |
| **Specialty** | Спеціальності (належать до кафедр) |
| **Group** | Академічні групи (належать до спеціальностей) |
| **Subject** | Дисципліни (викладаються кафедрами) |
| **Course** | Курс = Subject + Teacher + Group + Semester |
| **Assignment** | Завдання створені викладачем для курсу |
| **Submission** | Роботи подані студентами |
| **Grade** | Оцінки виставлені викладачем |

Детальні схеми та зв'язки дивіться в [ARCHITECTURE.md](ARCHITECTURE.md).

---

## 🔐 Авторизація та ролі

### Ролі користувачів (UserRole enum)
```csharp
public enum UserRole
{
    Admin = 0,      // Адміністратор
    Teacher = 1,    // Викладач
    Student = 2     // Студент
}
```

### Матриця доступу

| Функція | Admin | Teacher | Student |
|---------|-------|---------|---------|
| Керування довідниками | ✅ | ❌ | ❌ |
| Створення користувачів | ✅ | ❌ | ❌ |
| Створення курсів | ✅ | ❌ | ❌ |
| Створення завдань | ❌ | ✅ (свої курси) | ❌ |
| Виставлення оцінок | ❌ | ✅ (свої курси) | ❌ |
| Подання робіт | ❌ | ❌ | ✅ (свої курси) |
| Перегляд всіх оцінок | ✅ | ✅ (свої курси) | ✅ (лише своїх) |

---

## 📝 Entity Framework Міграції

### Основні команди

```bash
# Створити нову міграцію
dotnet ef migrations add MigrationName

# Застосувати міграції до бази
dotnet ef database update

# Відкотити до попередньої міграції
dotnet ef database update PreviousMigrationName

# Видалити останню міграцію (якщо не застосована)
dotnet ef migrations remove

# Генерація SQL скрипту
dotnet ef migrations script

# Видалення бази даних
dotnet ef database drop
```

### Історія міграцій
- `20260527135425_InitialMigration` - Початкова структура БД ✅

---

## 🧪 Тестування

```bash
# Запустити всі тести
cd tests/MyRACIT.Tests
dotnet test

# Запустити з покриттям коду
dotnet test /p:CollectCoverage=true
```

---

## 🗺 Roadmap

### ✅ Фаза 1: Фундамент (Поточна)
- [x] Створення моделей Entity
- [x] Налаштування DbContext
- [x] Початкова міграція
- [x] Базова структура MVC
- [x] UML-діаграми
- [x] Документація Use Cases

### 🔄 Фаза 2: Основний функціонал
- [ ] Імплементація ASP.NET Identity
- [ ] CRUD операції для всіх сутностей
- [ ] Панелі адміністратора (CoreAdmin)
- [ ] Інтерфейс викладача
- [ ] Інтерфейс студента

### ⏳ Фаза 3: Навчальний процес
- [ ] Система завдань та подань
- [ ] Виставлення оцінок
- [ ] Зворотний зв'язок
- [ ] Календар та дедлайни
- [ ] Email нотифікації

### 📅 Фаза 4: Аналітика та звіти
- [ ] Dashboard з статистикою
- [ ] Графіки успішності
- [ ] Експорт у Excel/PDF
- [ ] Журнал відвідувань
- [ ] Генерація звітів

### 🚀 Фаза 5: Розширений функціонал
- [ ] REST API
- [ ] Mobile-friendly версія
- [ ] Інтеграція з зовнішніми системами
- [ ] Онлайн чат підтримки
- [ ] Багатомовність (UA/EN)

---

## 🤝 Внесок у проєкт

Ми раді будь-якому внеску! Якщо ви хочете допомогти:

1. Fork репозиторій
2. Створіть feature branch (`git checkout -b feature/AmazingFeature`)
3. Commit зміни (`git commit -m 'Add some AmazingFeature'`)
4. Push в branch (`git push origin feature/AmazingFeature`)
5. Відкрийте Pull Request

### Coding Standards
- Використовуйте C# naming conventions
- Додавайте XML коментарі до публічних методів
- Пишіть unit тести для нового функціоналу
- Слідуйте SOLID принципам

---

## 📄 Ліцензія

Цей проєкт ліцензовано під MIT License - дивіться файл [LICENSE](LICENSE) для деталей.

---

## 👨‍💻 Автори

- **Ваше ім'я** - *Початкова робота* - [YourGitHub](https://github.com/yourusername)

---

## 🙏 Подяки

- [ASP.NET Core Documentation](https://docs.microsoft.com/aspnet/core/)
- [Entity Framework Core](https://docs.microsoft.com/ef/core/)
- [CoreAdmin Library](https://github.com/edandersen/core-admin)
- [Bootstrap](https://getbootstrap.com/)

---

## 📞 Контакти

**Email:** support@myracit.com  
**GitHub Issues:** [https://github.com/yourusername/my-racit-app/issues](https://github.com/yourusername/my-racit-app/issues)  
**Documentation:** [https://docs.myracit.com](https://docs.myracit.com)

---

<div align="center">
  <strong>Зроблено з ❤️ для РФКІТ</strong>
</div>

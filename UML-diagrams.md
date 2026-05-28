# UML-діаграми для MyRACIT

## 1. Use Case діаграма (Діаграма варіантів використання)

```mermaid
graph TB
    subgraph "Інформаційна система MyRACIT"
        %% Адміністративні функції
        UC1[Керування кафедрами]
        UC2[Керування спеціальностями]
        UC3[Керування групами]
        UC4[Керування дисциплінами]
        UC5[Створення облікових записів студентів]
        UC6[Створення облікових записів викладачів]
        UC7[Реєстрація студентів на курси]
        
        %% Функції викладача
        UC8[Перегляд списку студентів курсу]
        UC9[Створення завдань]
        UC10[Виставлення оцінок]
        UC11[Надання зворотного зв'язку]
        UC12[Перегляд поданих робіт]
        
        %% Функції студента
        UC13[Заповнення профілю]
        UC14[Перегляд своїх курсів]
        UC15[Перегляд завдань]
        UC16[Подання робіт]
        UC17[Перегляд своїх оцінок]
        
        %% Загальні функції
        UC18[Авторизація в системі]
        UC19[Перегляд розкладу]
    end
    
    Admin[👤 Адміністратор]
    Teacher[👤 Викладач]
    Student[👤 Студент]
    
    %% Адміністратор
    Admin --> UC1
    Admin --> UC2
    Admin --> UC3
    Admin --> UC4
    Admin --> UC5
    Admin --> UC6
    Admin --> UC7
    Admin --> UC18
    
    %% Викладач
    Teacher --> UC8
    Teacher --> UC9
    Teacher --> UC10
    Teacher --> UC11
    Teacher --> UC12
    Teacher --> UC18
    Teacher --> UC19
    
    %% Студент
    Student --> UC13
    Student --> UC14
    Student --> UC15
    Student --> UC16
    Student --> UC17
    Student --> UC18
    Student --> UC19
    
    style Admin fill:#ff6b6b
    style Teacher fill:#4ecdc4
    style Student fill:#95e1d3
```

## 2. Class діаграма (Діаграма класів)

### 2.1. Виділення класів предметної області

Система MyRACIT містить наступні класи:
- **User** - базовий клас користувача системи
- **UserRole** - enum для ролей користувачів
- **StudentProfile** - профіль студента
- **TeacherProfile** - профіль викладача
- **Department** - кафедра закладу освіти
- **Specialty** - спеціальність
- **Group** - навчальна група
- **Subject** - навчальна дисципліна (предмет)
- **Course** - семестровий курс (зв'язок предмету, викладача та групи)
- **Assignment** - навчальне завдання
- **AssignmentFile** - файл прикріплений до завдання
- **AssignmentLink** - посилання прикріплене до завдання
- **Submission** - здана робота студента
- **Grade** - оцінка за здану роботу

### 2.2. Атрибути та методи класів

```mermaid
classDiagram
    %% ============ ENUM ============
    class UserRole {
        <<enumeration>>
        Admin
        Teacher
        Student
    }
    
    %% ============ CORE ENTITIES ============
    class User {
        -int Id
        -string PasswordHash
        -string Name
        -string Email
        -UserRole Role
        +ToString() string
    }
    
    class StudentProfile {
        -int Id
        -int UserId
        -int GroupId
        +User User
        +Group Group
    }
    
    class TeacherProfile {
        -int Id
        -int UserId
        -int DepartmentId
        +User User
        +Department Department
        +ToString() string
    }
    
    %% ============ ORGANIZATION ============
    class Department {
        -int Id
        -string Name
        +ToString() string
    }
    
    class Specialty {
        -int Id
        -string Code
        -string Name
        +ToString() string
    }
    
    class Group {
        -int Id
        -string Name
        -int StudyYear
        -int SpecialtyId
        +Specialty Specialty
        +ToString() string
    }
    
    class Subject {
        -int Id
        -string Title
        +ToString() string
    }
    
    %% ============ ACADEMIC PROCESS ============
    class Course {
        -int Id
        -int SubjectId
        -int TeacherId
        -int GroupId
        -DateTime StartDate
        -DateTime EndDate
        +Subject Subject
        +TeacherProfile Teacher
        +Group Group
        +ToString() string
    }
    
    class Assignment {
        -int Id
        -string Title
        -string Description
        -DateTime Deadline
        -int MaxGrade
        -int CourseId
        +Course Course
        +ToString() string
    }
    
    class AssignmentFile {
        -int Id
        -int AssignmentId
        -string FilePath
        -string FileName
        -DateTime UploadedAt
        +Assignment Assignment
        +ToString() string
    }
    
    class AssignmentLink {
        -int Id
        -int AssignmentId
        -string Url
        -string Label
        +Assignment Assignment
        +ToString() string
    }
    
    class Submission {
        -int Id
        -int AssignmentId
        -int StudentId
        -string FilePath
        -DateTime SubmittedAt
        +Assignment Assignment
        +StudentProfile Student
        +ToString() string
    }
    
    class Grade {
        -int Id
        -int StudentId
        -int SubmissionId
        -int Value
        -DateTime DateIssued
        -string Feedback
        +StudentProfile Student
        +Submission Submission
        +ToString() string
    }
    
    %% ============ RELATIONSHIPS ============
    
    %% User and Roles (Association)
    User --> UserRole : має роль
    
    %% User and Profiles (Composition - профіль не існує без користувача)
    User "1" *-- "0..1" StudentProfile : має профіль студента
    User "1" *-- "0..1" TeacherProfile : має профіль викладача
    
    %% Department relationships (Aggregation - кафедра може існувати без викладачів)
    Department "1" o-- "0..*" TeacherProfile : працює на
    
    %% Specialty and Group (Composition - група завжди належить спеціальності)
    Specialty "1" *-- "0..*" Group : містить групи
    
    %% Group and Students (Aggregation - група може існувати без студентів)
    Group "1" o-- "0..*" StudentProfile : навчається в
    
    %% Course relationships (Association)
    Subject "1" -- "0..*" Course : викладається як
    TeacherProfile "1" -- "0..*" Course : веде
    Group "1" -- "0..*" Course : проводиться для
    
    %% Assignment relationships (Composition - завдання не існує без курсу)
    Course "1" *-- "0..*" Assignment : має завдання
    
    %% Assignment resources (Composition - файли/посилання не існують без завдання)
    Assignment "1" *-- "0..*" AssignmentFile : має файли
    Assignment "1" *-- "0..*" AssignmentLink : має посилання
    
    %% Submission relationships (Composition - подання не існує без завдання)
    Assignment "1" *-- "0..*" Submission : отримує подання
    StudentProfile "1" -- "0..*" Submission : подає роботу
    
    %% Grade relationships (Composition - оцінка не існує без подання)
    Submission "1" *-- "0..1" Grade : оцінюється
    StudentProfile "1" -- "0..*" Grade : отримує оцінку
```

### 2.3. Типи зв'язків між класами

**Асоціація (Association)** `--`:
- User → UserRole: користувач має роль
- Subject → Course: предмет викладається як курс
- TeacherProfile → Course: викладач веде курс
- Group → Course: група відвідує курс
- StudentProfile → Submission: студент подає роботу
- StudentProfile → Grade: студент отримує оцінку

**Агрегація (Aggregation)** `o--` (ціле може існувати без частин):
- Department o-- TeacherProfile: кафедра може існувати без викладачів
- Group o-- StudentProfile: група може існувати без студентів

**Композиція (Composition)** `*--` (ціле не може існувати без частин, або частина не існує без цілого):
- User *-- StudentProfile: профіль студента не існує без користувача
- User *-- TeacherProfile: профіль викладача не існує без користувача
- Specialty *-- Group: група завжди належить спеціальності
- Course *-- Assignment: завдання не існує без курсу
- Assignment *-- AssignmentFile: файл не існує без завдання
- Assignment *-- AssignmentLink: посилання не існує без завдання
- Assignment *-- Submission: подання не існує без завдання
- Submission *-- Grade: оцінка не існує без конкретної зданої роботи

**Наслідування (Inheritance)**: 
- Не використовується в даній системі (всі класи незалежні)

### 2.4. Пояснення структури класів

#### Базові класи (Core Entities):
- **User** - центральний клас системи, що представляє користувача. Має роль (Admin/Teacher/Student) та базову інформацію (ім'я, email, пароль)
- **StudentProfile** / **TeacherProfile** - розширюють функціональність User додатковою інформацією про студента або викладача
- **UserRole** - enum для строгої типізації ролей

#### Організаційна структура (Organization):
- **Department** - кафедра закладу освіти
- **Specialty** - спеціальність (наприклад, "Інженерія програмного забезпечення")
- **Group** - академічна група студентів (наприклад, "ІПЗ-21")
- **Subject** - навчальна дисципліна (наприклад, "Програмування на C#")

#### Навчальний процес (Academic Process):
- **Course** - семестровий курс, що зв'язує Subject, Teacher та Group
- **Assignment** - завдання для курсу (домашня робота або лабораторна)
- **AssignmentFile** - файли прикріплені до завдання (презентації, методички)
- **AssignmentLink** - посилання на ресурси для завдання
- **Submission** - здана робота студента
- **Grade** - оцінка за конкретну здану роботу

#### Ключові рішення архітектури:
1. **Grade прив'язаний до Submission**, а не Assignment - це дозволяє оцінювати конкретну здачу, підтримувати перездачі
2. **AssignmentFile та AssignmentLink** - окремі таблиці для матеріалів завдання (масштабованість)
3. **Composition для Grade-Submission** - оцінка не може існувати без конкретної зданої роботи
4. **Aggregation для Group-Student** - група може існувати без студентів (на початку семестру)

## 3. ER-діаграма (Entity-Relationship)

```mermaid
erDiagram
    %% ============ USER MANAGEMENT ============
    User ||--o| StudentProfile : "має профіль"
    User ||--o| TeacherProfile : "має профіль"
    
    %% ============ ORGANIZATION ============
    Department ||--o{ TeacherProfile : "працюють на"
    
    Specialty ||--o{ Group : "містить групи"
    
    Group ||--o{ StudentProfile : "навчаються в"
    Group ||--o{ Course : "відвідують"
    
    %% ============ ACADEMIC PROCESS ============
    Subject ||--o{ Course : "викладається як"
    TeacherProfile ||--o{ Course : "веде курс"
    
    Course ||--o{ Assignment : "має завдання"
    
    Assignment ||--o{ AssignmentFile : "має файли"
    Assignment ||--o{ AssignmentLink : "має посилання"
    Assignment ||--o{ Submission : "отримує здачі"
    
    StudentProfile ||--o{ Submission : "подає роботи"
    
    Submission ||--o| Grade : "оцінюється"
    StudentProfile ||--o{ Grade : "отримує оцінки"
    
    %% ============ ENTITY DEFINITIONS ============
    User {
        int Id PK
        string PasswordHash
        string Name
        string Email
        enum Role
    }
    
    StudentProfile {
        int Id PK
        int UserId FK
        int GroupId FK
    }
    
    TeacherProfile {
        int Id PK
        int UserId FK
        int DepartmentId FK
    }
    
    Department {
        int Id PK
        string Name
    }
    
    Specialty {
        int Id PK
        string Code
        string Name
    }
    
    Group {
        int Id PK
        string Name
        int StudyYear
        int SpecialtyId FK
    }
    
    Subject {
        int Id PK
        string Title
    }
    
    Course {
        int Id PK
        int SubjectId FK
        int TeacherId FK
        int GroupId FK
        DateTime StartDate
        DateTime EndDate
    }
    
    Assignment {
        int Id PK
        string Title
        string Description
        DateTime Deadline
        int MaxGrade
        int CourseId FK
    }
    
    AssignmentFile {
        int Id PK
        int AssignmentId FK
        string FilePath
        string FileName
        DateTime UploadedAt
    }
    
    AssignmentLink {
        int Id PK
        int AssignmentId FK
        string Url
        string Label
    }
    
    Submission {
        int Id PK
        int AssignmentId FK
        int StudentId FK
        string FilePath
        DateTime SubmittedAt
    }
    
    Grade {
        int Id PK
        int StudentId FK
        int SubmissionId FK
        int Value
        DateTime DateIssued
        string Feedback
    }
```

## 4. Sequence діаграма: Виставлення оцінки викладачем

```mermaid
sequenceDiagram
    actor Teacher as 👤 Викладач
    participant UI as Веб-інтерфейс
    participant Controller as Controller
    participant DB as База даних
    
    Teacher->>UI: Авторизація
    UI->>Controller: Перевірка облікових даних
    Controller->>DB: Запит User by Email
    DB-->>Controller: Дані користувача
    Controller-->>UI: Успішна авторизація
    
    Teacher->>UI: Обирає курс
    UI->>Controller: GET /Teacher/Course/{id}
    Controller->>DB: Запит Course + Students
    DB-->>Controller: Дані курсу та студентів
    Controller-->>UI: Відображення студентів
    
    Teacher->>UI: Обирає завдання
    UI->>Controller: GET /Teacher/Assignment/{id}
    Controller->>DB: Запит Assignment + Submissions
    DB-->>Controller: Завдання та подані роботи
    Controller-->>UI: Список поданих робіт
    
    Teacher->>UI: Вводить оцінку та коментар
    UI->>Controller: POST /Teacher/Grade
    Controller->>DB: INSERT Grade
    DB-->>Controller: Grade збережено
    Controller-->>UI: Успішне збереження
    UI-->>Teacher: Підтвердження виставлення оцінки
```

## 5. Activity діаграма: Реєстрація студента на курс

```mermaid
flowchart TD
    Start([Початок]) --> Login[Адміністратор входить в систему]
    Login --> SelectStudent[Обирає студента]
    SelectStudent --> CheckGroup{Студент<br/>прив'язаний<br/>до групи?}
    
    CheckGroup -->|Ні| AssignGroup[Призначити студента до групи]
    AssignGroup --> SelectCourse
    CheckGroup -->|Так| SelectCourse[Обрати доступний курс]
    
    SelectCourse --> CheckEnrollment{Студент вже<br/>записаний на<br/>цей курс?}
    
    CheckEnrollment -->|Так| ShowError[Показати помилку:<br/>вже записаний]
    ShowError --> End([Кінець])
    
    CheckEnrollment -->|Ні| CheckCapacity{Курс має<br/>вільні місця?}
    
    CheckCapacity -->|Ні| ShowErrorFull[Показати помилку:<br/>курс заповнений]
    ShowErrorFull --> End
    
    CheckCapacity -->|Так| EnrollStudent[Записати студента на курс]
    EnrollStudent --> UpdateDB[(Оновити базу даних)]
    UpdateDB --> SendNotification[Надіслати повідомлення студенту]
    SendNotification --> ShowSuccess[Показати успішне повідомлення]
    ShowSuccess --> End
```

## 6. Component діаграма: Архітектура системи

```mermaid
graph TB
    subgraph "Presentation Layer"
        Views[Views<br/>Razor Pages]
        Controllers[Controllers<br/>MVC]
        WWW[wwwroot<br/>Static Files]
    end
    
    subgraph "Business Logic Layer"
        Services[Services<br/>Business Logic]
        Models[Models<br/>Domain Entities]
    end
    
    subgraph "Data Access Layer"
        Context[DbContext<br/>EF Core]
        Migrations[Migrations]
    end
    
    subgraph "External"
        MSSQL[(MS SQL Server<br/>Database)]
        CoreAdmin[CoreAdmin<br/>Admin Panel]
    end
    
    Views <--> Controllers
    Controllers <--> Services
    Services <--> Models
    Services <--> Context
    Context <--> Migrations
    Context <--> MSSQL
    CoreAdmin <--> Context
    WWW --> Views
    
    style Views fill:#e1f5ff
    style Controllers fill:#e1f5ff
    style Services fill:#fff4e1
    style Models fill:#fff4e1
    style Context fill:#ffe1f5
    style MSSQL fill:#f0f0f0
    style CoreAdmin fill:#e1ffe1
```

## Опис компонентів системи

### Presentation Layer (Рівень презентації)
- **Views**: Razor-шаблони для відображення UI
- **Controllers**: MVC-контролери для обробки запитів
- **wwwroot**: Статичні файли (CSS, JS, зображення)

### Business Logic Layer (Рівень бізнес-логіки)
- **Services**: Сервіси з бізнес-логікою
- **Models**: Доменні моделі (Entity classes)

### Data Access Layer (Рівень доступу до даних)
- **DbContext**: Entity Framework Core контекст
- **Migrations**: Міграції бази даних

### External Components (Зовнішні компоненти)
- **MS SQL Server**: Реляційна база даних
- **CoreAdmin**: Бібліотека для адміністративної панелі

## Основні сценарії використання

### Адміністратор:
1. Керування довідниками (кафедри, спеціальності, дисципліни)
2. Створення та управління групами
3. Реєстрація користувачів (студентів і викладачів)
4. Призначення студентів до груп
5. Створення курсів та призначення викладачів

### Викладач:
1. Перегляд закріплених курсів
2. Створення завдань для курсу
3. Перегляд поданих робіт студентів
4. Виставлення оцінок
5. Надання зворотного зв'язку

### Студент:
1. Перегляд своїх курсів
2. Перегляд завдань
3. Подання виконаних робіт
4. Перегляд отриманих оцінок та коментарів
5. Заповнення та редагування профілю

## 7. Service Layer діаграма: Сервіси та їх інтерфейси

```mermaid
classDiagram
    %% ============ INTERFACES ============
    class IUserService {
        <<interface>>
        +CreateStudentAsync(name, email, password, groupId) Task~StudentProfile~
        +CreateTeacherAsync(name, email, password, departmentId) Task~TeacherProfile~
        +CreateAdminAsync(name, email, password) Task~User~
        +AuthenticateAsync(email, password) Task~User~
        +ChangePasswordAsync(userId, oldPassword, newPassword) Task
        +EmailExistsAsync(email) Task~bool~
        +GetStudentProfileAsync(userId) Task~StudentProfile~
        +GetTeacherProfileAsync(userId) Task~TeacherProfile~
        +UpdateUserProfileAsync(userId, name, email) Task
        +DeleteUserAsync(userId) Task
    }
    
    class IFileStorageService {
        <<interface>>
        +SaveFileAsync(file, folder) Task~string~
        +GetFileAsync(filePath) Task~byte[]~
        +DeleteFileAsync(filePath) Task
        +FileExists(filePath) bool
        +GetFileSize(filePath) long
        +GetContentType(fileName) string
    }
    
    class IEmailService {
        <<interface>>
        +SendEmailAsync(to, subject, body) Task
        +SendCredentialsAsync(to, name, email, password, role) Task
        +SendNewCourseNotificationAsync(to, studentName, courseName, teacherName) Task
        +SendNewAssignmentNotificationAsync(to, studentName, assignmentTitle, dueDate) Task
        +SendGradeNotificationAsync(to, studentName, assignmentTitle, points, maxPoints, feedback) Task
    }
    
    %% ============ IMPLEMENTATIONS ============
    class UserService {
        -MyRacitDbContext _context
        +CreateStudentAsync(name, email, password, groupId) Task~StudentProfile~
        +CreateTeacherAsync(name, email, password, departmentId) Task~TeacherProfile~
        +CreateAdminAsync(name, email, password) Task~User~
        +AuthenticateAsync(email, password) Task~User~
        +ChangePasswordAsync(userId, oldPassword, newPassword) Task
        +EmailExistsAsync(email) Task~bool~
        +GetStudentProfileAsync(userId) Task~StudentProfile~
        +GetTeacherProfileAsync(userId) Task~TeacherProfile~
        +UpdateUserProfileAsync(userId, name, email) Task
        +DeleteUserAsync(userId) Task
        -HashPassword(password) string
        -VerifyPassword(password, hash) bool
    }
    
    class FileStorageService {
        -IWebHostEnvironment _environment
        -string _uploadsFolderPath
        +SaveFileAsync(file, folder) Task~string~
        +GetFileAsync(filePath) Task~byte[]~
        +DeleteFileAsync(filePath) Task
        +FileExists(filePath) bool
        +GetFileSize(filePath) long
        +GetContentType(fileName) string
        -EnsureDirectoryExists(path) void
        -GenerateUniqueFileName(originalFileName) string
    }
    
    class EmailService {
        -IConfiguration _configuration
        -ILogger~EmailService~ _logger
        +SendEmailAsync(to, subject, body) Task
        +SendCredentialsAsync(to, name, email, password, role) Task
        +SendNewCourseNotificationAsync(to, studentName, courseName, teacherName) Task
        +SendNewAssignmentNotificationAsync(to, studentName, assignmentTitle, dueDate) Task
        +SendGradeNotificationAsync(to, studentName, assignmentTitle, points, maxPoints, feedback) Task
        -CreateSmtpClient() SmtpClient
        -BuildHtmlEmail(template, data) string
    }
    
    %% ============ CONTROLLERS ============
    class AdminController {
        -IUserService _userService
        -IEmailService _emailService
        -MyRacitDbContext _context
        +CreateStudent() IActionResult
        +CreateTeacher() IActionResult
        +DeleteUser(userId) IActionResult
    }
    
    class AuthController {
        -IUserService _userService
        +Login(email, password) IActionResult
        +Logout() IActionResult
        +ChangePassword() IActionResult
    }
    
    class TeacherController {
        -IFileStorageService _fileStorageService
        -IEmailService _emailService
        -MyRacitDbContext _context
        +UploadAssignmentFile() IActionResult
        +DownloadFile() IActionResult
        +GradeSubmission() IActionResult
    }
    
    class StudentController {
        -IFileStorageService _fileStorageService
        -MyRacitDbContext _context
        +SubmitAssignment() IActionResult
        +DownloadSubmission() IActionResult
        +DeleteSubmission() IActionResult
    }
    
    %% ============ DATA CONTEXT ============
    class MyRacitDbContext {
        +DbSet~User~ Users
        +DbSet~StudentProfile~ StudentProfiles
        +DbSet~TeacherProfile~ TeacherProfiles
        +DbSet~Assignment~ Assignments
        +DbSet~Submission~ Submissions
        +DbSet~Grade~ Grades
        +SaveChangesAsync() Task
    }
    
    %% ============ DTOs ============
    class CreateStudentDto {
        +string Name
        +string Email
        +string Password
        +int GroupId
    }
    
    class CreateTeacherDto {
        +string Name
        +string Email
        +string Password
        +int DepartmentId
    }
    
    class LoginDto {
        +string Email
        +string Password
        +bool RememberMe
    }
    
    %% ============ RELATIONSHIPS ============
    %% Implementation relationships
    IUserService <|.. UserService : implements
    IFileStorageService <|.. FileStorageService : implements
    IEmailService <|.. EmailService : implements
    
    %% Service dependencies
    UserService --> MyRacitDbContext : uses
    FileStorageService --> IWebHostEnvironment : uses
    EmailService --> IConfiguration : uses
    EmailService --> ILogger : uses
    
    %% Controller dependencies
    AdminController --> IUserService : uses
    AdminController --> IEmailService : uses
    AdminController --> MyRacitDbContext : uses
    
    AuthController --> IUserService : uses
    
    TeacherController --> IFileStorageService : uses
    TeacherController --> IEmailService : uses
    TeacherController --> MyRacitDbContext : uses
    
    StudentController --> IFileStorageService : uses
    StudentController --> MyRacitDbContext : uses
    
    %% DTOs usage
    AdminController --> CreateStudentDto : uses
    AdminController --> CreateTeacherDto : uses
    AuthController --> LoginDto : uses
    
    %% Service interactions
    AdminController ..> UserService : CreateStudentAsync()
    AdminController ..> EmailService : SendCredentialsAsync()
    TeacherController ..> FileStorageService : SaveFileAsync()
    StudentController ..> FileStorageService : SaveFileAsync()
```

## 8. Sequence діаграма: Створення студента з надсиланням credentials

```mermaid
sequenceDiagram
    actor Admin as 👤 Адміністратор
    participant Controller as AdminController
    participant UserService as IUserService
    participant EmailService as IEmailService
    participant DB as MyRacitDbContext
    participant SMTP as SMTP Server
    
    Admin->>Controller: POST /Admin/CreateStudent(CreateStudentDto)
    activate Controller
    
    %% Перевірка унікальності email
    Controller->>UserService: EmailExistsAsync(email)
    activate UserService
    UserService->>DB: SELECT User WHERE Email = ?
    DB-->>UserService: bool (exists/not)
    UserService-->>Controller: false (email вільний)
    deactivate UserService
    
    %% Створення користувача + профілю
    Controller->>UserService: CreateStudentAsync(name, email, password, groupId)
    activate UserService
    Note over UserService: Хешує пароль SHA256
    UserService->>DB: BEGIN TRANSACTION
    UserService->>DB: INSERT INTO Users
    DB-->>UserService: userId
    UserService->>DB: INSERT INTO StudentProfiles
    DB-->>UserService: studentProfileId
    UserService->>DB: COMMIT TRANSACTION
    UserService-->>Controller: StudentProfile (з User)
    deactivate UserService
    
    %% Надсилання credentials email
    Controller->>EmailService: SendCredentialsAsync(email, name, password, "Student")
    activate EmailService
    Note over EmailService: Створює HTML-шаблон листа
    EmailService->>SMTP: SendMailAsync(to, subject, htmlBody)
    activate SMTP
    SMTP-->>EmailService: Email відправлено
    deactivate SMTP
    EmailService-->>Controller: Task completed
    deactivate EmailService
    
    Controller-->>Admin: RedirectToAction("Students") + TempData["Success"]
    deactivate Controller
    
    Note over Admin: Бачить повідомлення:<br/>"Студента створено!<br/>Credentials надіслано на email"
```

## 9. Sequence діаграма: Подання роботи з файлом

```mermaid
sequenceDiagram
    actor Student as 👤 Студент
    participant Controller as StudentController
    participant FileService as IFileStorageService
    participant DB as MyRacitDbContext
    participant FS as File System
    
    Student->>Controller: POST /Student/SubmitAssignment(assignmentId, content, file)
    activate Controller
    
    %% Перевірка дедлайну
    Controller->>DB: SELECT Assignment WHERE Id = ?
    DB-->>Controller: Assignment (з DueDate)
    Note over Controller: Перевіряє:<br/>DateTime.Now <= DueDate
    
    alt Дедлайн минув
        Controller-->>Student: Error: "Дедлайн минув!"
    else Дедлайн актуальний
        
        %% Перевірка на перездачу
        Controller->>DB: SELECT Submission WHERE AssignmentId = ? AND StudentId = ?
        DB-->>Controller: existing Submission (or null)
        
        alt Існує попередня здача
            Note over Controller: Видаляє стару Grade
            Controller->>DB: DELETE Grade WHERE SubmissionId = ?
            DB-->>Controller: OK
            
            %% Видалення старого файлу
            Controller->>FileService: DeleteFileAsync(oldFilePath)
            activate FileService
            FileService->>FS: File.Delete(path)
            FS-->>FileService: OK
            FileService-->>Controller: Task completed
            deactivate FileService
            
            Controller->>DB: DELETE Submission WHERE Id = ?
            DB-->>Controller: OK
        end
        
        %% Збереження нового файлу
        alt Файл прикріплено
            Controller->>FileService: SaveFileAsync(file, "submissions")
            activate FileService
            Note over FileService: Генерує Guid ім'я<br/>Перевіряє розмір <= 10MB
            FileService->>FS: Directory.CreateDirectory("wwwroot/uploads/submissions")
            FS-->>FileService: OK
            FileService->>FS: File.WriteAllBytes(uniqueFileName, bytes)
            FS-->>FileService: OK
            FileService-->>Controller: "uploads/submissions/guid_filename.pdf"
            deactivate FileService
        end
        
        %% Створення нової здачі
        Controller->>DB: INSERT Submission (AssignmentId, StudentId, Content, FilePath)
        DB-->>Controller: submissionId
        
        Controller-->>Student: RedirectToAction("MySubmissions") + TempData["Success"]
        deactivate Controller
        
        Note over Student: Бачить повідомлення:<br/>"Роботу успішно подано!"
    end
```

## 10. Sequence діаграма: Оцінювання роботи з надсиланням сповіщення

```mermaid
sequenceDiagram
    actor Teacher as 👤 Викладач
    participant Controller as TeacherController
    participant EmailService as IEmailService
    participant DB as MyRacitDbContext
    participant SMTP as SMTP Server
    
    Teacher->>Controller: POST /Teacher/GradeSubmission(submissionId, points, feedback)
    activate Controller
    
    %% Отримання даних
    Controller->>DB: SELECT Submission with Assignment and Student
    DB-->>Controller: Submission (з MaxPoints, StudentEmail, AssignmentTitle)
    
    %% Валідація балів
    Note over Controller: Перевіряє:<br/>0 <= points <= MaxPoints
    
    alt Бали некоректні
        Controller-->>Teacher: Error: "Бали поза межами!"
    else Бали коректні
        
        %% Перевірка на існуючу оцінку
        Controller->>DB: SELECT Grade WHERE SubmissionId = ?
        DB-->>Controller: existing Grade (or null)
        
        alt Оцінка вже існує
            Controller->>DB: UPDATE Grade SET Points = ?, Feedback = ?
            DB-->>Controller: OK
        else Нова оцінка
            Controller->>DB: INSERT Grade (SubmissionId, StudentId, Points, Feedback)
            DB-->>Controller: gradeId
        end
        
        %% Надсилання email сповіщення
        Controller->>EmailService: SendGradeNotificationAsync(studentEmail, studentName, assignmentTitle, points, maxPoints, feedback)
        activate EmailService
        
        Note over EmailService: Створює HTML-лист<br/>з кольоровою індикацією:<br/>🟢 >= 75%<br/>🟡 >= 50%<br/>🔴 < 50%
        
        EmailService->>SMTP: SendMailAsync(to, subject, htmlBody)
        activate SMTP
        SMTP-->>EmailService: Email відправлено
        deactivate SMTP
        
        EmailService-->>Controller: Task completed
        deactivate EmailService
        
        Controller-->>Teacher: RedirectToAction("Submissions") + TempData["Success"]
        deactivate Controller
        
        Note over Teacher: Бачить повідомлення:<br/>"Оцінку виставлено!<br/>Студента сповіщено"
    end
```

## 11. Service Layer: Патерни проектування

### Factory Method Pattern (UserService)

```mermaid
classDiagram
    class IUserService {
        <<interface>>
        +CreateStudentAsync() Task~StudentProfile~
        +CreateTeacherAsync() Task~TeacherProfile~
        +CreateAdminAsync() Task~User~
    }
    
    class UserService {
        +CreateStudentAsync() Task~StudentProfile~
        +CreateTeacherAsync() Task~TeacherProfile~
        +CreateAdminAsync() Task~User~
        -CreateUserAsync(name, email, password, role) Task~User~
    }
    
    class User {
        +int Id
        +string Name
        +string Email
        +UserRole Role
    }
    
    class StudentProfile {
        +int UserId
        +int GroupId
        +User User
    }
    
    class TeacherProfile {
        +int UserId
        +int DepartmentId
        +User User
    }
    
    IUserService <|.. UserService : implements
    UserService --> User : creates
    UserService --> StudentProfile : creates
    UserService --> TeacherProfile : creates
    StudentProfile --> User : has
    TeacherProfile --> User : has
    
    note for UserService "Factory Method Pattern:<br/>Створює різні типи користувачів<br/>з відповідними профілями<br/>в одній транзакції"
```

**Переваги Factory Method Pattern:**
- ✅ Інкапсуляція логіки створення користувачів
- ✅ Автоматичне хешування паролів
- ✅ Транзакційне створення User + Profile
- ✅ Централізована валідація email
- ✅ Легке розширення новими типами користувачів

### Strategy Pattern (FileStorageService)

```mermaid
classDiagram
    class IFileStorageService {
        <<interface>>
        +SaveFileAsync(file, folder) Task~string~
        +GetFileAsync(filePath) Task~byte[]~
        +DeleteFileAsync(filePath) Task
        +GetContentType(fileName) string
    }
    
    class FileStorageService {
        +SaveFileAsync(file, folder) Task~string~
        +GetFileAsync(filePath) Task~byte[]~
        +DeleteFileAsync(filePath) Task
        +GetContentType(fileName) string
    }
    
    class LocalStorageStrategy {
        -string _uploadsFolderPath
        +SaveToLocal(file, folder) string
        +ReadFromLocal(filePath) byte[]
        +DeleteFromLocal(filePath) void
    }
    
    class CloudStorageStrategy {
        -string _cloudEndpoint
        +SaveToCloud(file, folder) string
        +ReadFromCloud(filePath) byte[]
        +DeleteFromCloud(filePath) void
    }
    
    IFileStorageService <|.. FileStorageService : implements
    FileStorageService --> LocalStorageStrategy : uses (current)
    FileStorageService ..> CloudStorageStrategy : can use (future)
    
    note for FileStorageService "Strategy Pattern готовий:<br/>Легко замінити локальне<br/>збереження на Cloud Storage<br/>(Azure Blob, AWS S3, тощо)"
```

**Переваги Strategy Pattern:**
- ✅ Легка заміна способу збереження файлів
- ✅ Тестування з in-memory storage
- ✅ Підтримка різних сховищ для різних файлів
- ✅ Масштабованість (локально → хмара)

### Template Method Pattern (EmailService)

```mermaid
classDiagram
    class IEmailService {
        <<interface>>
        +SendEmailAsync(to, subject, body) Task
        +SendCredentialsAsync(...) Task
        +SendGradeNotificationAsync(...) Task
    }
    
    class EmailService {
        +SendEmailAsync(to, subject, body) Task
        +SendCredentialsAsync(...) Task
        +SendGradeNotificationAsync(...) Task
        -BuildEmailTemplate(templateType, data) string
        -CreateSmtpClient() SmtpClient
    }
    
    class CredentialsTemplate {
        +BuildHtml(name, email, password, role) string
    }
    
    class GradeNotificationTemplate {
        +BuildHtml(studentName, grade, feedback) string
    }
    
    class AssignmentNotificationTemplate {
        +BuildHtml(assignmentTitle, dueDate) string
    }
    
    IEmailService <|.. EmailService : implements
    EmailService --> CredentialsTemplate : uses
    EmailService --> GradeNotificationTemplate : uses
    EmailService --> AssignmentNotificationTemplate : uses
    
    note for EmailService "Template Method Pattern:<br/>Загальний алгоритм відправки<br/>+ специфічні HTML-шаблони<br/>для різних типів листів"
```

**Переваги Template Method Pattern:**
- ✅ Єдиний метод відправки (DRY)
- ✅ Легке додавання нових типів листів
- ✅ Централізоване логування помилок
- ✅ Fallback якщо SMTP не налаштований

## 12. Архітектура застосунку: повна картина

```mermaid
graph TB
    subgraph "Client Layer"
        Browser[🌐 Web Browser]
    end
    
    subgraph "Presentation Layer (MVC)"
        Views[📄 Views<br/>Razor Pages]
        Controllers[🎮 Controllers<br/>Admin, Teacher,<br/>Student, Auth]
        DTOs[📦 DTOs<br/>Request/Response<br/>Models]
    end
    
    subgraph "Business Logic Layer"
        subgraph "Services"
            UserService[👤 UserService<br/>Factory Method]
            FileService[📁 FileStorageService<br/>Strategy Pattern]
            EmailService[✉️ EmailService<br/>Template Method]
        end
        
        Interfaces[📋 Interfaces<br/>IUserService<br/>IFileStorageService<br/>IEmailService]
    end
    
    subgraph "Domain Layer"
        Models[🗂️ Domain Models<br/>User, Course,<br/>Assignment, Grade, etc.]
    end
    
    subgraph "Data Access Layer"
        DbContext[🔌 MyRacitDbContext<br/>EF Core]
        Migrations[⚙️ Migrations]
    end
    
    subgraph "Infrastructure"
        Database[(🗄️ SQL Server<br/>myracitdb)]
        FileSystem[💾 File System<br/>wwwroot/uploads/]
        SMTP[📧 SMTP Server<br/>Gmail, SendGrid]
    end
    
    %% Connections
    Browser <-->|HTTP/HTTPS| Views
    Views <--> Controllers
    Controllers --> DTOs
    Controllers --> Interfaces
    
    Interfaces <--> UserService
    Interfaces <--> FileService
    Interfaces <--> EmailService
    
    UserService --> Models
    FileService --> Models
    EmailService --> Models
    
    UserService --> DbContext
    DbContext --> Models
    DbContext --> Migrations
    DbContext <--> Database
    
    FileService <--> FileSystem
    EmailService <--> SMTP
    
    style Browser fill:#e3f2fd
    style Views fill:#bbdefb
    style Controllers fill:#90caf9
    style DTOs fill:#64b5f6
    style UserService fill:#fff9c4
    style FileService fill:#fff59d
    style EmailService fill:#fff176
    style Interfaces fill:#ffee58
    style Models fill:#ffccbc
    style DbContext fill:#ffab91
    style Database fill:#90a4ae
    style FileSystem fill:#b0bec5
    style SMTP fill:#cfd8dc
```

### Опис архітектури:

**Client Layer:**
- Web Browser - користувацький інтерфейс (Chrome, Firefox, Edge)

**Presentation Layer (MVC):**
- **Views** - Razor Pages для відображення UI
- **Controllers** - обробка HTTP-запитів, валідація, маршрутизація
- **DTOs** - Data Transfer Objects для передачі даних між шарами

**Business Logic Layer:**
- **IUserService** / **UserService** - управління користувачами (Factory Method)
- **IFileStorageService** / **FileStorageService** - робота з файлами (Strategy)
- **IEmailService** / **EmailService** - відправка email (Template Method)

**Domain Layer:**
- **Models** - доменні сутності (User, Course, Assignment, Grade, etc.)

**Data Access Layer:**
- **MyRacitDbContext** - Entity Framework Core контекст
- **Migrations** - міграції бази даних

**Infrastructure:**
- **SQL Server** - реляційна база даних
- **File System** - локальне збереження файлів
- **SMTP Server** - сервер для відправки email

### Переваги архітектури:

✅ **Separation of Concerns** - кожен шар має свою відповідальність
✅ **Dependency Injection** - всі сервіси через інтерфейси
✅ **Testability** - легко писати Unit-тести для сервісів
✅ **Maintainability** - зміни в одному шарі не впливають на інші
✅ **Scalability** - можна легко замінити File System на Cloud Storage
✅ **Security** - Authentication/Authorization на рівні Controllers

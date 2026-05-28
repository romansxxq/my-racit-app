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

```mermaid
classDiagram
    class User {
        +int Id
        +string PasswordHash
        +string Name
        +string Email
        +UserRole Role
        +ToString() string
    }
    
    class UserRole {
        <<enumeration>>
        Admin
        Teacher
        Student
    }
    
    class StudentProfile {
        +int Id
        +int UserId
        +int GroupId
        +User User
        +Group Group
    }
    
    class TeacherProfile {
        +int Id
        +int UserId
        +int DepartmentId
        +User User
        +Department Department
        +ToString() string
    }
    
    class Department {
        +int Id
        +string Name
        +string Code
        +ToString() string
    }
    
    class Specialty {
        +int Id
        +string Name
        +string Code
        +int DepartmentId
        +Department Department
        +ToString() string
    }
    
    class Group {
        +int Id
        +string Name
        +int StudyYear
        +int SpecialtyId
        +Specialty Specialty
        +ToString() string
    }
    
    class Subject {
        +int Id
        +string Title
        +string Code
        +string Description
        +int Credits
        +int DepartmentId
        +Department Department
        +ToString() string
    }
    
    class Course {
        +int Id
        +int SubjectId
        +int TeacherId
        +int GroupId
        +DateTime StartDate
        +DateTime EndDate
        +Subject Subject
        +TeacherProfile Teacher
        +Group Group
        +ToString() string
    }
    
    class Assignment {
        +int Id
        +string Title
        +string Description
        +DateTime Deadline
        +int MaxGrade
        +int CourseId
        +Course Course
        +ToString() string
    }
    
    class Submission {
        +int Id
        +int StudentId
        +int AssignmentId
        +DateTime SubmittedAt
        +string Content
        +string FilePath
        +StudentProfile Student
        +Assignment Assignment
    }
    
    class Grade {
        +int Id
        +int StudentId
        +int AssignmentId
        +int Value
        +DateTime DateIssued
        +string Feedback
        +StudentProfile Student
        +Assignment Assignment
        +ToString() string
    }
    
    %% Relationships
    User "1" --> "1" UserRole : має роль
    User "1" <-- "0..1" StudentProfile : профіль студента
    User "1" <-- "0..1" TeacherProfile : профіль викладача
    
    Department "1" <-- "*" TeacherProfile : працює на
    Department "1" <-- "*" Specialty : належить до
    Department "1" <-- "*" Subject : викладається на
    
    Specialty "1" <-- "*" Group : навчається за
    
    Group "1" <-- "*" StudentProfile : навчається в
    Group "1" <-- "*" Course : проводиться для
    
    Subject "1" <-- "*" Course : викладається як
    TeacherProfile "1" <-- "*" Course : веде
    
    Course "1" <-- "*" Assignment : має завдання
    
    StudentProfile "1" <-- "*" Submission : подає роботу
    Assignment "1" <-- "*" Submission : має подання
    
    StudentProfile "1" <-- "*" Grade : отримує оцінку
    Assignment "1" <-- "*" Grade : оцінюється за
```

## 3. ER-діаграма (Entity-Relationship)

```mermaid
erDiagram
    User ||--o| StudentProfile : "має"
    User ||--o| TeacherProfile : "має"
    User ||--|| UserRole : "належить до"
    
    Department ||--o{ TeacherProfile : "працюють"
    Department ||--o{ Specialty : "містить"
    Department ||--o{ Subject : "викладає"
    
    Specialty ||--o{ Group : "навчаються"
    
    Group ||--o{ StudentProfile : "навчаються в"
    Group ||--o{ Course : "відвідують"
    
    Subject ||--o{ Course : "є курсом"
    TeacherProfile ||--o{ Course : "веде"
    
    Course ||--o{ Assignment : "має завдання"
    
    StudentProfile ||--o{ Submission : "подає"
    Assignment ||--o{ Submission : "отримує подання"
    
    StudentProfile ||--o{ Grade : "отримує"
    Assignment ||--o{ Grade : "оцінюється"
    
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
        string Code
    }
    
    Specialty {
        int Id PK
        string Name
        string Code
        int DepartmentId FK
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
        string Code
        string Description
        int Credits
        int DepartmentId FK
    }
    
    Course {
        int Id PK
        int SubjectId FK
        int TeacherId FK
        int GroupId FK
        date StartDate
        date EndDate
    }
    
    Assignment {
        int Id PK
        string Title
        string Description
        datetime Deadline
        int MaxGrade
        int CourseId FK
    }
    
    Submission {
        int Id PK
        int StudentId FK
        int AssignmentId FK
        datetime SubmittedAt
        string Content
        string FilePath
    }
    
    Grade {
        int Id PK
        int StudentId FK
        int AssignmentId FK
        int Value
        datetime DateIssued
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

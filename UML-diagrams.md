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

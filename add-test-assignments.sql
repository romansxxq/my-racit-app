-- Додавання тестових завдань до курсу
-- Спочатку знайдемо ID першого курсу
DECLARE @CourseId INT = (SELECT TOP 1 Id FROM Courses ORDER BY Id);

-- Перевірка чи курс існує
IF @CourseId IS NOT NULL
BEGIN
    -- Додаємо тестові завдання
    INSERT INTO Assignments (CourseId, Title, Description, Deadline, MaxGrade, Type, CreatedAt)
    VALUES 
    (@CourseId, 
     N'Лабораторна робота №1: Основи C#', 
     N'Створіть консольний додаток з використанням базових конструкцій C#',
     DATEADD(day, 14, GETDATE()),
     100,
     1, -- Lab
     GETDATE()),
    
    (@CourseId,
     N'Домашнє завдання №1: Змінні та типи даних',
     N'Виконайте вправи на роботу зі змінними',
     DATEADD(day, 7, GETDATE()),
     50,
     0, -- Homework
     GETDATE()),
    
    (@CourseId,
     N'Лабораторна робота №2: ООП в C#',
     N'Розробіть класову модель для предметної області',
     DATEADD(day, 21, GETDATE()),
     100,
     1, -- Lab
     GETDATE()),
    
    (@CourseId,
     N'Проєкт: Розробка веб-застосунку',
     N'Створіть повнофункціональний веб-застосунок з використанням ASP.NET Core',
     DATEADD(day, 60, GETDATE()),
     200,
     2, -- Project
     GETDATE());
    
    PRINT N'✅ Додано 4 тестових завдання';
END
ELSE
BEGIN
    PRINT N'❌ Курси не знайдені в базі даних';
END

-- Перевірка результату
SELECT 
    a.Id,
    a.Title,
    a.Type,
    a.MaxGrade,
    a.Deadline,
    c.Id as CourseId,
    s.Title as SubjectTitle
FROM Assignments a
INNER JOIN Courses c ON a.CourseId = c.Id
INNER JOIN Subjects s ON c.SubjectId = s.Id
ORDER BY a.CreatedAt DESC;

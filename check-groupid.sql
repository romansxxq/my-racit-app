-- Перевірка GroupId студента та курсів
-- Виконайте цей запит щоб побачити які GroupId використовуються

-- 1. Перевіряємо тестового студента
SELECT 
    u.Email,
    u.Name,
    sp.Id AS StudentId,
    sp.GroupId AS StudentGroupId,
    g.Name AS GroupName
FROM StudentProfiles sp
INNER JOIN Users u ON sp.UserId = u.Id
INNER JOIN Groups g ON sp.GroupId = g.Id
WHERE u.Email = 'shevchenko@rfkit.edu.ua';

-- 2. Перевіряємо курси
SELECT 
    c.Id AS CourseId,
    s.Title AS SubjectTitle,
    c.GroupId AS CourseGroupId,
    g.Name AS GroupName
FROM Courses c
INNER JOIN Subjects s ON c.SubjectId = s.Id
INNER JOIN Groups g ON c.GroupId = g.Id
ORDER BY c.Id;

-- 3. Перевіряємо завдання
SELECT 
    a.Id AS AssignmentId,
    a.Title,
    a.CourseId,
    c.GroupId AS CourseGroupId,
    g.Name AS GroupName
FROM Assignments a
INNER JOIN Courses c ON a.CourseId = c.Id
INNER JOIN Groups g ON c.GroupId = g.Id
ORDER BY a.Id DESC;

-- 4. Перевіряємо групи
SELECT Id, Name FROM Groups;

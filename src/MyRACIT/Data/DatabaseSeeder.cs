using MyRACIT.Data;
using MyRACIT.Models.Entities;
using System.Security.Cryptography;
using System.Text;

namespace MyRACIT.Data
{
    /// <summary>
    /// Клас для заповнення бази даних тестовими даними
    /// </summary>
    public static class DatabaseSeeder
    {
        public static async Task SeedAsync(MyRacitDbContext context)
        {
            // Перевірка чи вже є дані
            if (context.Users.Any())
            {
                return; // База вже заповнена
            }

            // ============ USERS AND PROFILES ============
            
            // Адміністратор
            var adminUser = new User
            {
                Name = "Адміністратор Системи",
                Email = "admin@rfkit.edu",
                PasswordHash = HashPassword("Admin123"),
                Role = UserRole.Admin
            };
            context.Users.Add(adminUser);
            await context.SaveChangesAsync();

            // ============ DEPARTMENTS ============
            var departments = new[]
            {
                new Department { Name = "Кафедра програмної інженерії", Description = "Розробка програмного забезпечення" },
                new Department { Name = "Кафедра комп'ютерних наук", Description = "Теоретичні основи інформатики" },
                new Department { Name = "Кафедра кібербезпеки", Description = "Захист інформації" }
            };
            context.Departments.AddRange(departments);
            await context.SaveChangesAsync();

            // ============ SPECIALTIES ============
            var specialties = new[]
            {
                new Specialty { Code = "121", Name = "Інженерія програмного забезпечення", Description = "Розробка ПЗ" },
                new Specialty { Code = "122", Name = "Комп'ютерні науки", Description = "Computer Science" },
                new Specialty { Code = "125", Name = "Кібербезпека", Description = "Information Security" }
            };
            context.Specialties.AddRange(specialties);
            await context.SaveChangesAsync();

            // ============ GROUPS ============
            var groups = new[]
            {
                new Group { Name = "ІПЗ-21", SpecialtyId = specialties[0].Id, Year = 2021, MaxStudents = 25 },
                new Group { Name = "ІПЗ-22", SpecialtyId = specialties[0].Id, Year = 2022, MaxStudents = 25 },
                new Group { Name = "КН-21", SpecialtyId = specialties[1].Id, Year = 2021, MaxStudents = 20 },
                new Group { Name = "КБ-21", SpecialtyId = specialties[2].Id, Year = 2021, MaxStudents = 20 }
            };
            context.Groups.AddRange(groups);
            await context.SaveChangesAsync();

            // ============ TEACHERS ============
            var teachers = new[]
            {
                new { Name = "Іванов Іван Іванович", Email = "ivanov@rfkit.edu", DepartmentId = departments[0].Id },
                new { Name = "Петренко Петро Петрович", Email = "petrenko@rfkit.edu", DepartmentId = departments[0].Id },
                new { Name = "Сидоренко Марія Олександрівна", Email = "sydorenko@rfkit.edu", DepartmentId = departments[1].Id }
            };

            var teacherProfiles = new List<TeacherProfile>();
            foreach (var t in teachers)
            {
                var user = new User
                {
                    Name = t.Name,
                    Email = t.Email,
                    PasswordHash = HashPassword("Teacher123"),
                    Role = UserRole.Teacher
                };
                context.Users.Add(user);
                await context.SaveChangesAsync();

                var profile = new TeacherProfile
                {
                    UserId = user.Id,
                    DepartmentId = t.DepartmentId,
                    HireDate = DateTime.Now.AddYears(-3)
                };
                context.TeacherProfiles.Add(profile);
                teacherProfiles.Add(profile);
            }
            await context.SaveChangesAsync();

            // ============ STUDENTS ============
            var students = new[]
            {
                new { Name = "Коваленко Олександр", Email = "kovalenko@student.rfkit.edu", GroupId = groups[0].Id },
                new { Name = "Шевченко Марина", Email = "shevchenko@student.rfkit.edu", GroupId = groups[0].Id },
                new { Name = "Бондаренко Андрій", Email = "bondarenko@student.rfkit.edu", GroupId = groups[0].Id },
                new { Name = "Мельник Олена", Email = "melnyk@student.rfkit.edu", GroupId = groups[1].Id },
                new { Name = "Ткаченко Дмитро", Email = "tkachenko@student.rfkit.edu", GroupId = groups[1].Id }
            };

            var studentProfiles = new List<StudentProfile>();
            foreach (var s in students)
            {
                var user = new User
                {
                    Name = s.Name,
                    Email = s.Email,
                    PasswordHash = HashPassword("Student123"),
                    Role = UserRole.Student
                };
                context.Users.Add(user);
                await context.SaveChangesAsync();

                var profile = new StudentProfile
                {
                    UserId = user.Id,
                    GroupId = s.GroupId,
                    EnrollmentYear = 2021
                };
                context.StudentProfiles.Add(profile);
                studentProfiles.Add(profile);
            }
            await context.SaveChangesAsync();

            // ============ SUBJECTS ============
            var subjects = new[]
            {
                new Subject { Title = "Програмування на C#", Description = "Основи ООП та C#", Credits = 5 },
                new Subject { Title = "Бази даних", Description = "SQL Server та EF Core", Credits = 4 },
                new Subject { Title = "Веб-розробка", Description = "ASP.NET Core MVC", Credits = 6 },
                new Subject { Title = "Алгоритми та структури даних", Description = "Основи алгоритмізації", Credits = 5 }
            };
            context.Subjects.AddRange(subjects);
            await context.SaveChangesAsync();

            // ============ COURSES ============
            var courses = new[]
            {
                new Course 
                { 
                    SubjectId = subjects[0].Id, 
                    TeacherId = teacherProfiles[0].Id, 
                    GroupId = groups[0].Id,
                    StartDate = new DateTime(2026, 9, 1),
                    EndDate = new DateTime(2027, 1, 31)
                },
                new Course 
                { 
                    SubjectId = subjects[1].Id, 
                    TeacherId = teacherProfiles[1].Id, 
                    GroupId = groups[0].Id,
                    StartDate = new DateTime(2026, 9, 1),
                    EndDate = new DateTime(2027, 1, 31)
                },
                new Course 
                { 
                    SubjectId = subjects[2].Id, 
                    TeacherId = teacherProfiles[0].Id, 
                    GroupId = groups[1].Id,
                    StartDate = new DateTime(2026, 9, 1),
                    EndDate = new DateTime(2027, 1, 31)
                }
            };
            context.Courses.AddRange(courses);
            await context.SaveChangesAsync();

            // ============ ASSIGNMENTS ============
            var assignments = new[]
            {
                new Assignment
                {
                    CourseId = courses[0].Id,
                    Title = "Лабораторна робота №1: Основи C#",
                    Description = "Створити консольний додаток з використанням ООП",
                    DueDate = DateTime.Now.AddDays(14),
                    MaxPoints = 100,
                    CreatedAt = DateTime.Now
                },
                new Assignment
                {
                    CourseId = courses[0].Id,
                    Title = "Лабораторна робота №2: Колекції",
                    Description = "Робота з List, Dictionary, LINQ",
                    DueDate = DateTime.Now.AddDays(21),
                    MaxPoints = 100,
                    CreatedAt = DateTime.Now
                },
                new Assignment
                {
                    CourseId = courses[1].Id,
                    Title = "Лабораторна робота №1: SQL запити",
                    Description = "Створити базу даних та написати SELECT запити",
                    DueDate = DateTime.Now.AddDays(10),
                    MaxPoints = 100,
                    CreatedAt = DateTime.Now
                }
            };
            context.Assignments.AddRange(assignments);
            await context.SaveChangesAsync();

            // ============ ASSIGNMENT LINKS ============
            var assignmentLinks = new[]
            {
                new AssignmentLink
                {
                    AssignmentId = assignments[0].Id,
                    Url = "https://docs.microsoft.com/en-us/dotnet/csharp/",
                    Label = "Документація C#"
                },
                new AssignmentLink
                {
                    AssignmentId = assignments[1].Id,
                    Url = "https://docs.microsoft.com/en-us/dotnet/api/system.collections.generic",
                    Label = "Документація по колекціях"
                }
            };
            context.AssignmentLinks.AddRange(assignmentLinks);
            await context.SaveChangesAsync();

            // ============ SUBMISSIONS ============
            var submissions = new[]
            {
                new Submission
                {
                    AssignmentId = assignments[0].Id,
                    StudentProfileId = studentProfiles[0].Id,
                    Content = "Виконав всі завдання. Додаток додано у вкладенні.",
                    SubmittedAt = DateTime.Now.AddDays(-2)
                },
                new Submission
                {
                    AssignmentId = assignments[0].Id,
                    StudentProfileId = studentProfiles[1].Id,
                    Content = "Лабораторна робота виконана повністю.",
                    SubmittedAt = DateTime.Now.AddDays(-1)
                }
            };
            context.Submissions.AddRange(submissions);
            await context.SaveChangesAsync();

            // ============ GRADES ============
            var grades = new[]
            {
                new Grade
                {
                    SubmissionId = submissions[0].Id,
                    StudentProfileId = studentProfiles[0].Id,
                    Points = 95,
                    Feedback = "Відмінна робота! Всі завдання виконані правильно.",
                    GradedAt = DateTime.Now.AddDays(-1)
                },
                new Grade
                {
                    SubmissionId = submissions[1].Id,
                    StudentProfileId = studentProfiles[1].Id,
                    Points = 85,
                    Feedback = "Добре, але є кілька помилок у коментарях коду.",
                    GradedAt = DateTime.Now
                }
            };
            context.Grades.AddRange(grades);
            await context.SaveChangesAsync();

            Console.WriteLine("✅ База даних успішно заповнена тестовими даними!");
            Console.WriteLine("📧 Облікові записи:");
            Console.WriteLine("   Адміністратор: admin@rfkit.edu / Admin123");
            Console.WriteLine("   Викладач: ivanov@rfkit.edu / Teacher123");
            Console.WriteLine("   Студент: kovalenko@student.rfkit.edu / Student123");
        }

        /// <summary>
        /// Хешування пароля SHA256 (для сумісності з UserService)
        /// </summary>
        private static string HashPassword(string password)
        {
            using var sha256 = SHA256.Create();
            var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
            return Convert.ToBase64String(bytes);
        }
    }
}

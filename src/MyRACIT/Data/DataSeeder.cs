using Microsoft.EntityFrameworkCore;
using MyRACIT.Models.Entities;
using MyRACIT.Services.Interfaces;

namespace MyRACIT.Data
{
    /// <summary>
    /// Клас для початкового наповнення бази даних тестовими даними
    /// </summary>
    public class DataSeeder
    {
        private readonly MyRacitDbContext _context;
        private readonly IUserService _userService;
        private readonly ILogger<DataSeeder> _logger;

        public DataSeeder(MyRacitDbContext context, IUserService userService, ILogger<DataSeeder> logger)
        {
            _context = context;
            _userService = userService;
            _logger = logger;
        }

        /// <summary>
        /// Головний метод для ініціалізації даних
        /// </summary>
        public async Task SeedAsync()
        {
            try
            {
                // Перевірка чи база вже наповнена
                if (await _context.Users.AnyAsync())
                {
                    _logger.LogInformation("База даних вже містить дані користувачів.");
                    
                    // Видаляємо тільки завдання зі старою 100-бальною системою
                    var oldAssignments = await _context.Assignments.Where(a => a.MaxGrade > 5).ToListAsync();
                    if (oldAssignments.Any())
                    {
                        _logger.LogInformation($"Видалення {oldAssignments.Count} старих завдань для оновлення системи оцінювання...");
                        
                        // Спочатку видаляємо пов'язані оцінки та здачі
                        var assignmentIds = oldAssignments.Select(a => a.Id).ToList();
                        var oldSubmissions = await _context.Submissions
                            .Where(s => assignmentIds.Contains(s.AssignmentId))
                            .ToListAsync();
                        
                        if (oldSubmissions.Any())
                        {
                            var submissionIds = oldSubmissions.Select(s => s.Id).ToList();
                            var oldGrades = await _context.Grades
                                .Where(g => submissionIds.Contains(g.SubmissionId))
                                .ToListAsync();
                            
                            _context.Grades.RemoveRange(oldGrades);
                            _context.Submissions.RemoveRange(oldSubmissions);
                        }
                        
                        _context.Assignments.RemoveRange(oldAssignments);
                        await _context.SaveChangesAsync();
                        _logger.LogInformation("✅ Старі завдання видалено");
                    }
                    
                    // Перевіряємо чи є достатньо завдань (менше 3 означає потрібно додати)
                    var assignmentsCount = await _context.Assignments.CountAsync();
                    if (assignmentsCount < 3)
                    {
                        _logger.LogInformation($"Знайдено {assignmentsCount} завдань. Додаємо тестові завдання...");
                        await SeedAssignmentsForExistingCoursesAsync();
                    }
                    else
                    {
                        _logger.LogInformation($"✅ База містить {assignmentsCount} завдань з 5-бальною системою.");
                    }
                    
                    return;
                }

                _logger.LogInformation("Початок наповнення бази даних...");

                //Створення кафедр
                await SeedDepartmentsAsync();

                //Створення спеціальностей
                await SeedSpecialtiesAsync();

                //Створення груп
                await SeedGroupsAsync();

                //Створення предметів
                await SeedSubjectsAsync();

                //Створення користувачів (Admin, Teachers, Students)
                await SeedUsersAsync();

                //Створення курсів
                await SeedCoursesAsync();

                _logger.LogInformation("База даних успішно наповнена!");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Помилка при наповненні бази даних");
                throw;
            }
        }

        private async Task SeedDepartmentsAsync()
        {
            var departments = new[]
            {
                new Department { Name = "Кафедра інформаційних технологій" },
                new Department { Name = "Кафедра програмної інженерії" },
                new Department { Name = "Кафедра комп'ютерних наук" }
            };

            await _context.Departments.AddRangeAsync(departments);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Створено {Count} кафедр", departments.Length);
        }

        private async Task SeedSpecialtiesAsync()
        {
            var specialties = new[]
            {
                new Specialty { Code = "121", Name = "Інженерія програмного забезпечення" },
                new Specialty { Code = "122", Name = "Комп'ютерні науки" },
                new Specialty { Code = "123", Name = "Комп'ютерна інженерія" }
            };

            await _context.Specialties.AddRangeAsync(specialties);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Створено {Count} спеціальностей", specialties.Length);
        }

        private async Task SeedGroupsAsync()
        {
            var ipzSpecialty = await _context.Specialties.FirstAsync(s => s.Code == "121");
            var cnSpecialty = await _context.Specialties.FirstAsync(s => s.Code == "122");

            var groups = new[]
            {
                new Group { Name = "ІПЗ-21", SpecialtyId = ipzSpecialty.Id, StudyYear = 2021 },
                new Group { Name = "ІПЗ-22", SpecialtyId = ipzSpecialty.Id, StudyYear = 2022 },
                new Group { Name = "КН-21", SpecialtyId = cnSpecialty.Id, StudyYear = 2021 },
                new Group { Name = "КН-22", SpecialtyId = cnSpecialty.Id, StudyYear = 2022 }
            };

            await _context.Groups.AddRangeAsync(groups);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Створено {Count} груп", groups.Length);
        }

        private async Task SeedSubjectsAsync()
        {
            var subjects = new[]
            {
                new Subject { Title = "Програмування на C#" },
                new Subject { Title = "Бази даних" },
                new Subject { Title = "Веб-розробка" },
                new Subject { Title = "Алгоритми та структури даних" },
                new Subject { Title = "Об'єктно-орієнтоване програмування" }
            };

            await _context.Subjects.AddRangeAsync(subjects);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Створено {Count} предметів", subjects.Length);
        }

        private async Task SeedUsersAsync()
        {
            // 1. Адміністратор
            var admin = await _userService.CreateAdminAsync(
                "Адміністратор Системи",
                "admin@rfkit.edu.ua",
                "Admin123"
            );
            _logger.LogInformation("Створено адміністратора: admin@rfkit.edu.ua / Admin123");

            // 2. Викладачі
            var itDepartment = await _context.Departments.FirstAsync(d => d.Name.Contains("інформаційних технологій"));
            var seDepartment = await _context.Departments.FirstAsync(d => d.Name.Contains("програмної інженерії"));

            var teacher1 = await _userService.CreateTeacherAsync(
                "Іванов Іван Іванович",
                "teacher@rfkit.edu.ua",
                "Teacher123",
                itDepartment.Id
            );

            var teacher2 = await _userService.CreateTeacherAsync(
                "Петренко Петро Петрович",
                "petrenko@rfkit.edu.ua",
                "Teacher123",
                seDepartment.Id
            );

            var teacher3 = await _userService.CreateTeacherAsync(
                "Сидоренко Ольга Миколаївна",
                "sydorenko@rfkit.edu.ua",
                "Teacher123",
                itDepartment.Id
            );

            _logger.LogInformation("Створено 3 викладачів");

            // 3. Студенти
            var ipz21Group = await _context.Groups.FirstAsync(g => g.Name == "ІПЗ-21");
            var ipz22Group = await _context.Groups.FirstAsync(g => g.Name == "ІПЗ-22");
            var kn21Group = await _context.Groups.FirstAsync(g => g.Name == "КН-21");

            var students = new[]
            {
                ("Шевченко Тарас Григорович", "student@rfkit.edu.ua", ipz21Group.Id),
                ("Коваленко Марія Олександрівна", "kovalenko@rfkit.edu.ua", ipz21Group.Id),
                ("Бондаренко Олексій Васильович", "bondarenko@rfkit.edu.ua", ipz21Group.Id),
                ("Мельник Анна Іванівна", "melnyk@rfkit.edu.ua", ipz22Group.Id),
                ("Ткаченко Дмитро Сергійович", "tkachenko@rfkit.edu.ua", ipz22Group.Id),
                ("Морозова Олена Петрівна", "morozova@rfkit.edu.ua", kn21Group.Id)
            };

            foreach (var (name, email, groupId) in students)
            {
                await _userService.CreateStudentAsync(name, email, "Student123", groupId);
            }

            _logger.LogInformation("✅ Створено {Count} студентів (пароль: Student123)", students.Length);
        }

        private async Task SeedCoursesAsync()
        {
            var csharpSubject = await _context.Subjects.FirstAsync(s => s.Title.Contains("C#"));
            var dbSubject = await _context.Subjects.FirstAsync(s => s.Title.Contains("Бази даних"));
            var webSubject = await _context.Subjects.FirstAsync(s => s.Title.Contains("Веб-розробка"));

            var teacher1 = await _context.TeacherProfiles.FirstAsync(t => t.User.Email == "teacher@rfkit.edu.ua");
            var teacher2 = await _context.TeacherProfiles.FirstAsync(t => t.User.Email == "petrenko@rfkit.edu.ua");
            var teacher3 = await _context.TeacherProfiles.FirstAsync(t => t.User.Email == "sydorenko@rfkit.edu.ua");

            var ipz21Group = await _context.Groups.FirstAsync(g => g.Name == "ІПЗ-21");
            var ipz22Group = await _context.Groups.FirstAsync(g => g.Name == "ІПЗ-22");
            var kn21Group = await _context.Groups.FirstAsync(g => g.Name == "КН-21");

            var courses = new[]
            {
                new Course
                {
                    SubjectId = csharpSubject.Id,
                    TeacherId = teacher1.Id,
                    GroupId = ipz21Group.Id,
                    StartDate = new DateTime(2026, 9, 1),
                    EndDate = new DateTime(2027, 1, 31)
                },
                new Course
                {
                    SubjectId = dbSubject.Id,
                    TeacherId = teacher2.Id,
                    GroupId = ipz21Group.Id,
                    StartDate = new DateTime(2026, 9, 1),
                    EndDate = new DateTime(2027, 1, 31)
                },
                new Course
                {
                    SubjectId = webSubject.Id,
                    TeacherId = teacher3.Id,
                    GroupId = ipz22Group.Id,
                    StartDate = new DateTime(2026, 9, 1),
                    EndDate = new DateTime(2027, 1, 31)
                },
                new Course
                {
                    SubjectId = csharpSubject.Id,
                    TeacherId = teacher1.Id,
                    GroupId = kn21Group.Id,
                    StartDate = new DateTime(2026, 9, 1),
                    EndDate = new DateTime(2027, 1, 31)
                }
            };

            await _context.Courses.AddRangeAsync(courses);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Створено {Count} курсів", courses.Length);

            // Додамо тестові завдання для першого курсу
            var firstCourse = courses[0];
            var assignments = new[]
            {
                new Assignment
                {
                    CourseId = firstCourse.Id,
                    Title = "Лабораторна робота №1: Основи C#",
                    Description = "Створіть консольний додаток з використанням базових конструкцій C#",
                    Deadline = DateTime.Now.AddDays(14),
                    MaxGrade = 5,
                    Type = AssignmentType.Lab,
                    CreatedAt = DateTime.Now
                },
                new Assignment
                {
                    CourseId = firstCourse.Id,
                    Title = "Домашнє завдання №1: Змінні та типи даних",
                    Description = "Виконайте вправи на роботу зі змінними",
                    Deadline = DateTime.Now.AddDays(7),
                    MaxGrade = 5,
                    Type = AssignmentType.Homework,
                    CreatedAt = DateTime.Now
                },
                new Assignment
                {
                    CourseId = firstCourse.Id,
                    Title = "Лабораторна робота №2: ООП в C#",
                    Description = "Розробіть класову модель для предметної області",
                    Deadline = DateTime.Now.AddDays(21),
                    MaxGrade = 5,
                    Type = AssignmentType.Lab,
                    CreatedAt = DateTime.Now
                },
                new Assignment
                {
                    CourseId = firstCourse.Id,
                    Title = "Проєкт: Розробка веб-застосунку",
                    Description = "Створіть повнофункціональний веб-застосунок з використанням ASP.NET Core",
                    Deadline = DateTime.Now.AddDays(60),
                    MaxGrade = 5,
                    Type = AssignmentType.Project,
                    CreatedAt = DateTime.Now
                }
            };

            await _context.Assignments.AddRangeAsync(assignments);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Створено {Count} завдань", assignments.Length);
        }

        /// <summary>
        /// Додає тестові завдання до існуючих курсів
        /// </summary>
        private async Task SeedAssignmentsForExistingCoursesAsync()
        {
            // Знаходимо курс для групи ІПЗ-21 (до якої належить тестовий студент)
            var ipz21Group = await _context.Groups.FirstOrDefaultAsync(g => g.Name == "ІПЗ-21");
            if (ipz21Group == null)
            {
                _logger.LogWarning("Група ІПЗ-21 не знайдена. Завдання не додано.");
                return;
            }
            
            var firstCourse = await _context.Courses
                .Include(c => c.Subject)
                .FirstOrDefaultAsync(c => c.GroupId == ipz21Group.Id);
            if (firstCourse == null)
            {
                _logger.LogWarning("Курси для групи ІПЗ-21 не знайдені. Завдання не додано.");
                return;
            }
            
            _logger.LogInformation($"Додаємо завдання до курсу '{firstCourse.Subject?.Title}' для групи ІПЗ-21");

            var assignments = new[]
            {
                new Assignment
                {
                    CourseId = firstCourse.Id,
                    Title = "Лабораторна робота №1: Основи C#",
                    Description = "Створіть консольний додаток з використанням базових конструкцій C#",
                    Deadline = DateTime.Now.AddDays(14),
                    MaxGrade = 5,
                    Type = AssignmentType.Lab,
                    CreatedAt = DateTime.Now
                },
                new Assignment
                {
                    CourseId = firstCourse.Id,
                    Title = "Домашнє завдання №1: Змінні та типи даних",
                    Description = "Виконайте вправи на роботу зі змінними",
                    Deadline = DateTime.Now.AddDays(7),
                    MaxGrade = 5,
                    Type = AssignmentType.Homework,
                    CreatedAt = DateTime.Now
                },
                new Assignment
                {
                    CourseId = firstCourse.Id,
                    Title = "Лабораторна робота №2: ООП в C#",
                    Description = "Розробіть класову модель для предметної області",
                    Deadline = DateTime.Now.AddDays(21),
                    MaxGrade = 5,
                    Type = AssignmentType.Lab,
                    CreatedAt = DateTime.Now
                },
                new Assignment
                {
                    CourseId = firstCourse.Id,
                    Title = "Проєкт: Розробка веб-застосунку",
                    Description = "Створіть повнофункціональний веб-застосунок з використанням ASP.NET Core",
                    Deadline = DateTime.Now.AddDays(60),
                    MaxGrade = 5,
                    Type = AssignmentType.Project,
                    CreatedAt = DateTime.Now
                }
            };

            await _context.Assignments.AddRangeAsync(assignments);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Додано {Count} тестових завдань до існуючого курсу", assignments.Length);
        }
    }
}

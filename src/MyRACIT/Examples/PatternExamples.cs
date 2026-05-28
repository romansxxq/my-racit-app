using MyRACIT.Data;
using MyRACIT.Factories;
using MyRACIT.Models.Entities;
using MyRACIT.Services;
using MyRACIT.Strategies;

namespace MyRACIT.Examples
{
    /// <summary>
    /// Приклади використання реалізованих патернів
    /// Для демонстрації та навчання
    /// </summary>
    public class PatternExamples
    {
        private readonly MyRacitDbContext _context;

        public PatternExamples(MyRacitDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Приклад використання Factory Method Pattern
        /// </summary>
        public async Task FactoryMethodExample()
        {
            Console.WriteLine("=== Factory Method Pattern ===\n");

            // Створення різних типів користувачів через фабрику
            var student = UserFactory.CreateStudent("Іванов Іван Іванович", "ivanov@student.rfkit.edu.ua");
            var teacher = UserFactory.CreateTeacher("Петров Петро Петрович", "petrov@rfkit.edu.ua");
            var admin = UserFactory.CreateAdmin("Сидоров Сидор Сидорович", "admin@rfkit.edu.ua", "SecurePassword123!");

            Console.WriteLine($"Студент: {student.Name}, роль: {student.Role}");
            Console.WriteLine($"Викладач: {teacher.Name}, роль: {teacher.Role}");
            Console.WriteLine($"Адміністратор: {admin.Name}, роль: {admin.Role}");

            // Збереження в БД через репозиторій
            var userRepository = new Repository<User>(_context);
            await userRepository.AddAsync(student);
            await userRepository.AddAsync(teacher);
            await userRepository.AddAsync(admin);

            Console.WriteLine("\n✅ Користувачі успішно створені та збережені!\n");
        }

        /// <summary>
        /// Приклад використання Repository Pattern
        /// </summary>
        public async Task RepositoryPatternExample()
        {
            Console.WriteLine("=== Repository Pattern ===\n");

            // Створення репозиторіїв для різних сутностей
            var userRepository = new Repository<User>(_context);
            var groupRepository = new Repository<Group>(_context);
            var courseRepository = new Repository<Course>(_context);

            // Отримання всіх користувачів
            var allUsers = await userRepository.GetAllAsync();
            Console.WriteLine($"Всього користувачів: {allUsers.Count}");

            // Пошук студентів
            var students = await userRepository.FindAsync(u => u.Role == UserRole.Student);
            Console.WriteLine($"Студентів: {students.Count}");

            // Пошук викладачів
            var teachers = await userRepository.FindAsync(u => u.Role == UserRole.Teacher);
            Console.WriteLine($"Викладачів: {teachers.Count}");

            // Перевірка існування
            var hasAdmin = await userRepository.ExistsAsync(u => u.Role == UserRole.Admin);
            Console.WriteLine($"Є адміністратор: {hasAdmin}");

            Console.WriteLine("\n✅ Repository Pattern працює!\n");
        }

        /// <summary>
        /// Приклад використання Strategy Pattern
        /// </summary>
        public async Task StrategyPatternExample()
        {
            Console.WriteLine("=== Strategy Pattern ===\n");

            var gradeRepository = new Repository<Grade>(_context);
            var gradeService = new GradeService(gradeRepository, _context);

            // Припустимо у нас є студент з ID = 1 та курс з ID = 1
            int studentId = 1;
            int courseId = 1;

            // Використання різних стратегій обчислення оцінок
            Console.WriteLine("Обчислення фінальної оцінки різними стратегіями:\n");

            // Стратегія 1: Середній бал
            gradeService.SetStrategy(new AverageGradeStrategy());
            var averageGrade = await gradeService.GetFinalGradeAsync(studentId, courseId);
            Console.WriteLine($"Середній бал: {averageGrade:F2}");

            // Стратегія 2: Зважений бал
            gradeService.SetStrategy(new WeightedGradeStrategy());
            var weightedGrade = await gradeService.GetFinalGradeAsync(studentId, courseId);
            Console.WriteLine($"Зважений бал: {weightedGrade:F2}");

            // Стратегія 3: Відсотковий бал
            gradeService.SetStrategy(new PercentageGradeStrategy());
            var percentageGrade = await gradeService.GetFinalGradeAsync(studentId, courseId);
            Console.WriteLine($"Відсотковий бал: {percentageGrade:F2}");

            // Порівняння всіх стратегій одразу
            Console.WriteLine("\nПорівняння стратегій:");
            var comparison = await gradeService.CompareStrategiesAsync(studentId, courseId);
            foreach (var (strategyName, grade) in comparison)
            {
                Console.WriteLine($"  {strategyName}: {grade:F2}");
            }

            Console.WriteLine("\n✅ Strategy Pattern працює!\n");
        }

        /// <summary>
        /// Комплексний приклад: створення студента, запис на курс, виставлення оцінки
        /// </summary>
        public async Task ComplexExample()
        {
            Console.WriteLine("=== Комплексний приклад ===\n");

            // 1. Створюємо студента через фабрику
            var student = UserFactory.CreateStudent("Коваленко Анна", "kovalen@student.rfkit.edu.ua");
            var userRepo = new Repository<User>(_context);
            await userRepo.AddAsync(student);
            Console.WriteLine($"1. Створено студента: {student.Name}");

            // 2. Створюємо профіль студента (припустимо група з ID=1 існує)
            var studentProfile = new StudentProfile
            {
                UserId = student.Id,
                GroupId = 1
            };
            var profileRepo = new Repository<StudentProfile>(_context);
            await profileRepo.AddAsync(studentProfile);
            Console.WriteLine($"2. Створено профіль студента для групи #{studentProfile.GroupId}");

            // 3. Виставляємо оцінку (припустимо завдання з ID=1 існує)
            var gradeService = new GradeService(new Repository<Grade>(_context), _context);
            try
            {
                var grade = await gradeService.IssueGradeAsync(
                    studentProfile.Id,
                    assignmentId: 1,
                    value: 5,
                    feedback: "Відмінна робота! Всі вимоги виконані."
                );
                Console.WriteLine($"3. Виставлено оцінку: {grade.Value}/5");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"3. Помилка при виставленні оцінки: {ex.Message}");
            }

            // 4. Обчислюємо середній бал
            gradeService.SetStrategy(new AverageGradeStrategy());
            var averageGrade = await gradeService.GetAverageGradeForStudentAsync(studentProfile.Id);
            Console.WriteLine($"4. Середній бал студента: {averageGrade:F2}");

            Console.WriteLine("\n✅ Комплексний приклад виконано!\n");
        }

        /// <summary>
        /// Запустити всі приклади
        /// </summary>
        public async Task RunAllExamples()
        {
            Console.WriteLine("\n" + new string('=', 60));
            Console.WriteLine("ДЕМОНСТРАЦІЯ ПАТЕРНІВ ПРОЄКТУВАННЯ В MyRACIT");
            Console.WriteLine(new string('=', 60) + "\n");

            try
            {
                await FactoryMethodExample();
                await RepositoryPatternExample();
                await StrategyPatternExample();
                await ComplexExample();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\n❌ Помилка: {ex.Message}");
                Console.WriteLine($"Деталі: {ex.StackTrace}");
            }

            Console.WriteLine(new string('=', 60));
            Console.WriteLine("ДЕМОНСТРАЦІЯ ЗАВЕРШЕНА");
            Console.WriteLine(new string('=', 60) + "\n");
        }
    }
}

using Microsoft.EntityFrameworkCore;
using MyRACIT.Data;
using MyRACIT.Models.Entities;
using MyRACIT.Strategies;

namespace MyRACIT.Services
{
    /// <summary>
    /// Сервіс для роботи з оцінками
    /// Використовує Strategy Pattern для різних способів обчислення
    /// </summary>
    public class GradeService
    {
        private readonly IRepository<Grade> _gradeRepository;
        private readonly MyRacitDbContext _context;
        private IGradeStrategy _strategy;

        public GradeService(IRepository<Grade> gradeRepository, MyRacitDbContext context)
        {
            _gradeRepository = gradeRepository;
            _context = context;
            // За замовчуванням використовуємо середній бал
            _strategy = new AverageGradeStrategy();
        }

        /// <summary>
        /// Встановити стратегію обчислення оцінок
        /// </summary>
        public void SetStrategy(IGradeStrategy strategy)
        {
            _strategy = strategy ?? throw new ArgumentNullException(nameof(strategy));
        }

        /// <summary>
        /// Отримати фінальну оцінку студента за курсом
        /// </summary>
        public async Task<double> GetFinalGradeAsync(int studentId, int courseId)
        {
            var grades = await _context.Grades
                .Include(g => g.Assignment)
                .Where(g => g.StudentId == studentId && g.Assignment.CourseId == courseId)
                .ToListAsync();

            return _strategy.CalculateFinalGrade(grades);
        }

        /// <summary>
        /// Отримати фінальну оцінку студента за завданням
        /// </summary>
        public async Task<double> GetAverageGradeForStudentAsync(int studentId)
        {
            var grades = await _context.Grades
                .Include(g => g.Assignment)
                .Where(g => g.StudentId == studentId)
                .ToListAsync();

            return _strategy.CalculateFinalGrade(grades);
        }

        /// <summary>
        /// Виставити оцінку студенту за завдання
        /// </summary>
        public async Task<Grade> IssueGradeAsync(int studentId, int assignmentId, int value, string? feedback = null)
        {
            // Перевірка чи існує вже оцінка
            var existingGrade = await _context.Grades
                .FirstOrDefaultAsync(g => g.StudentId == studentId && g.AssignmentId == assignmentId);

            if (existingGrade != null)
            {
                throw new InvalidOperationException($"Оцінка за завдання #{assignmentId} вже виставлена студенту #{studentId}");
            }

            // Перевірка що оцінка в межах допустимого
            var assignment = await _context.Assignments.FindAsync(assignmentId);
            if (assignment == null)
            {
                throw new ArgumentException($"Завдання #{assignmentId} не знайдено", nameof(assignmentId));
            }

            if (value < 0 || value > assignment.MaxGrade)
            {
                throw new ArgumentException($"Оцінка має бути від 0 до {assignment.MaxGrade}", nameof(value));
            }

            var grade = new Grade
            {
                StudentId = studentId,
                AssignmentId = assignmentId,
                Value = value,
                DateIssued = DateTime.UtcNow,
                Feedback = feedback
            };

            await _gradeRepository.AddAsync(grade);
            
            Console.WriteLine($"[INFO] Виставлено оцінку {value}/{assignment.MaxGrade} студенту #{studentId} за завдання #{assignmentId}");

            return grade;
        }

        /// <summary>
        /// Оновити оцінку та коментар
        /// </summary>
        public async Task UpdateGradeAsync(int gradeId, int newValue, string? feedback = null)
        {
            var grade = await _context.Grades
                .Include(g => g.Assignment)
                .FirstOrDefaultAsync(g => g.Id == gradeId);

            if (grade == null)
            {
                throw new ArgumentException($"Оцінка #{gradeId} не знайдена", nameof(gradeId));
            }

            if (newValue < 0 || newValue > grade.Assignment.MaxGrade)
            {
                throw new ArgumentException($"Оцінка має бути від 0 до {grade.Assignment.MaxGrade}", nameof(newValue));
            }

            grade.Value = newValue;
            grade.Feedback = feedback;
            grade.DateIssued = DateTime.UtcNow;

            await _gradeRepository.UpdateAsync(grade);
            
            Console.WriteLine($"[INFO] Оновлено оцінку #{gradeId} до значення {newValue}");
        }

        /// <summary>
        /// Отримати статистику оцінок по курсу
        /// </summary>
        public async Task<CourseGradeStatistics> GetCourseStatisticsAsync(int courseId)
        {
            var grades = await _context.Grades
                .Include(g => g.Assignment)
                .Where(g => g.Assignment.CourseId == courseId)
                .ToListAsync();

            if (!grades.Any())
            {
                return new CourseGradeStatistics
                {
                    CourseId = courseId,
                    TotalGrades = 0,
                    AverageGrade = 0,
                    MinGrade = 0,
                    MaxGrade = 0
                };
            }

            return new CourseGradeStatistics
            {
                CourseId = courseId,
                TotalGrades = grades.Count,
                AverageGrade = Math.Round(grades.Average(g => g.Value), 2),
                MinGrade = grades.Min(g => g.Value),
                MaxGrade = grades.Max(g => g.Value)
            };
        }

        /// <summary>
        /// Порівняти різні стратегії оцінювання для студента
        /// </summary>
        public async Task<Dictionary<string, double>> CompareStrategiesAsync(int studentId, int courseId)
        {
            var grades = await _context.Grades
                .Include(g => g.Assignment)
                .Where(g => g.StudentId == studentId && g.Assignment.CourseId == courseId)
                .ToListAsync();

            var strategies = new IGradeStrategy[]
            {
                new AverageGradeStrategy(),
                new WeightedGradeStrategy(),
                new PercentageGradeStrategy()
            };

            var results = new Dictionary<string, double>();

            foreach (var strategy in strategies)
            {
                results[strategy.StrategyName] = strategy.CalculateFinalGrade(grades);
            }

            return results;
        }
    }

    /// <summary>
    /// Статистика оцінок по курсу
    /// </summary>
    public class CourseGradeStatistics
    {
        public int CourseId { get; set; }
        public int TotalGrades { get; set; }
        public double AverageGrade { get; set; }
        public int MinGrade { get; set; }
        public int MaxGrade { get; set; }
    }
}

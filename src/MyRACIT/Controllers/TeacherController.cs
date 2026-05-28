using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyRACIT.Data;
using MyRACIT.Models.Entities;
using MyRACIT.Services;
using MyRACIT.Strategies;

namespace MyRACIT.Controllers
{
    /// <summary>
    /// Контролер для викладача
    /// Робота з оцінками через Strategy Pattern
    /// </summary>
    public class TeacherController : Controller
    {
        private readonly IRepository<Course> _courseRepository;
        private readonly IRepository<Assignment> _assignmentRepository;
        private readonly GradeService _gradeService;
        private readonly MyRacitDbContext _context;

        public TeacherController(
            IRepository<Course> courseRepository,
            IRepository<Assignment> assignmentRepository,
            GradeService gradeService,
            MyRacitDbContext context)
        {
            _courseRepository = courseRepository;
            _assignmentRepository = assignmentRepository;
            _gradeService = gradeService;
            _context = context;
        }

        // GET: Teacher/Index
        public async Task<IActionResult> Index()
        {
            // TODO: Отримувати курси поточного викладача
            var courses = await _context.Courses
                .Include(c => c.Subject)
                .Include(c => c.Group)
                .Include(c => c.Teacher)
                .ThenInclude(t => t.User)
                .ToListAsync();

            return View(courses);
        }

        // GET: Teacher/Course/5
        public async Task<IActionResult> Course(int id)
        {
            var course = await _context.Courses
                .Include(c => c.Subject)
                .Include(c => c.Group)
                .Include(c => c.Teacher)
                .ThenInclude(t => t.User)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (course == null)
            {
                TempData["Error"] = "Курс не знайдено!";
                return RedirectToAction(nameof(Index));
            }

            // Отримати студентів групи
            var students = await _context.StudentProfiles
                .Include(s => s.User)
                .Where(s => s.GroupId == course.GroupId)
                .ToListAsync();

            ViewBag.Students = students;
            ViewBag.Course = course;

            return View(course);
        }

        // GET: Teacher/IssueGrade?studentId=1&courseId=1
        public async Task<IActionResult> IssueGrade(int studentId, int courseId)
        {
            var assignments = await _context.Assignments
                .Where(a => a.CourseId == courseId)
                .ToListAsync();

            ViewBag.StudentId = studentId;
            ViewBag.CourseId = courseId;
            ViewBag.Assignments = assignments;

            return View();
        }

        // POST: Teacher/IssueGrade
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> IssueGrade(int studentId, int assignmentId, int value, string? feedback)
        {
            try
            {
                await _gradeService.IssueGradeAsync(studentId, assignmentId, value, feedback);
                TempData["Success"] = "Оцінку успішно виставлено!";
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Помилка: {ex.Message}";
            }

            var assignment = await _context.Assignments.FindAsync(assignmentId);
            return RedirectToAction(nameof(Course), new { id = assignment?.CourseId });
        }

        // GET: Teacher/StudentGrades/5?courseId=1
        public async Task<IActionResult> StudentGrades(int id, int courseId)
        {
            var student = await _context.StudentProfiles
                .Include(s => s.User)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (student == null)
            {
                TempData["Error"] = "Студента не знайдено!";
                return RedirectToAction(nameof(Index));
            }

            var grades = await _context.Grades
                .Include(g => g.Assignment)
                .ThenInclude(a => a.Course)
                .Where(g => g.StudentId == id && g.Assignment.CourseId == courseId)
                .ToListAsync();

            // Використання Strategy Pattern для обчислення фінальної оцінки
            ViewBag.Student = student;
            ViewBag.CourseId = courseId;

            // Порівняння різних стратегій
            var strategies = await _gradeService.CompareStrategiesAsync(id, courseId);
            ViewBag.Strategies = strategies;

            return View(grades);
        }

        // GET: Teacher/CourseStatistics/5
        public async Task<IActionResult> CourseStatistics(int id)
        {
            var course = await _context.Courses
                .Include(c => c.Subject)
                .Include(c => c.Group)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (course == null)
            {
                TempData["Error"] = "Курс не знайдено!";
                return RedirectToAction(nameof(Index));
            }

            // Використання Strategy Pattern
            var statistics = await _gradeService.GetCourseStatisticsAsync(id);

            ViewBag.Course = course;
            ViewBag.Statistics = statistics;

            return View();
        }

        // GET: Teacher/ChangeStrategy
        public IActionResult ChangeStrategy()
        {
            ViewBag.Strategies = new List<string>
            {
                "AverageGradeStrategy",
                "WeightedGradeStrategy",
                "PercentageGradeStrategy"
            };
            return View();
        }

        // POST: Teacher/ChangeStrategy
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ChangeStrategy(string strategyName)
        {
            IGradeStrategy strategy = strategyName switch
            {
                "WeightedGradeStrategy" => new WeightedGradeStrategy(),
                "PercentageGradeStrategy" => new PercentageGradeStrategy(),
                _ => new AverageGradeStrategy()
            };

            _gradeService.SetStrategy(strategy);
            TempData["Success"] = $"Стратегію змінено на: {strategy.StrategyName}";
            
            return RedirectToAction(nameof(Index));
        }
    }
}

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyRACIT.Data;
using MyRACIT.Services;

namespace MyRACIT.Controllers
{
    /// <summary>
    /// Контролер для студента
    /// Перегляд курсів та оцінок
    /// </summary>
    public class StudentController : Controller
    {
        private readonly MyRacitDbContext _context;
        private readonly GradeService _gradeService;

        public StudentController(MyRacitDbContext context, GradeService gradeService)
        {
            _context = context;
            _gradeService = gradeService;
        }

        // GET: Student/Index
        public IActionResult Index()
        {
            // TODO: Отримати ID поточного студента з авторизації
            return View();
        }

        // GET: Student/MyCourses?studentId=1
        public async Task<IActionResult> MyCourses(int studentId)
        {
            var student = await _context.StudentProfiles
                .Include(s => s.User)
                .Include(s => s.Group)
                .FirstOrDefaultAsync(s => s.Id == studentId);

            if (student == null)
            {
                TempData["Error"] = "Студента не знайдено!";
                return RedirectToAction(nameof(Index));
            }

            // Отримати курси групи студента
            var courses = await _context.Courses
                .Include(c => c.Subject)
                .Include(c => c.Teacher)
                .ThenInclude(t => t.User)
                .Where(c => c.GroupId == student.GroupId)
                .ToListAsync();

            ViewBag.Student = student;
            return View(courses);
        }

        // GET: Student/MyGrades?studentId=1&courseId=1
        public async Task<IActionResult> MyGrades(int studentId, int courseId)
        {
            var grades = await _context.Grades
                .Include(g => g.Assignment)
                .ThenInclude(a => a.Course)
                .ThenInclude(c => c.Subject)
                .Where(g => g.StudentId == studentId && g.Assignment.CourseId == courseId)
                .OrderByDescending(g => g.DateIssued)
                .ToListAsync();

            var course = await _context.Courses
                .Include(c => c.Subject)
                .FirstOrDefaultAsync(c => c.Id == courseId);

            // Обчислення фінальної оцінки різними стратегіями
            var strategies = await _gradeService.CompareStrategiesAsync(studentId, courseId);

            ViewBag.Course = course;
            ViewBag.Strategies = strategies;

            return View(grades);
        }

        // GET: Student/AllGrades?studentId=1
        public async Task<IActionResult> AllGrades(int studentId)
        {
            var student = await _context.StudentProfiles
                .Include(s => s.User)
                .Include(s => s.Group)
                .FirstOrDefaultAsync(s => s.Id == studentId);

            if (student == null)
            {
                TempData["Error"] = "Студента не знайдено!";
                return RedirectToAction(nameof(Index));
            }

            var grades = await _context.Grades
                .Include(g => g.Assignment)
                .ThenInclude(a => a.Course)
                .ThenInclude(c => c.Subject)
                .Where(g => g.StudentId == studentId)
                .OrderByDescending(g => g.DateIssued)
                .ToListAsync();

            // Загальний середній бал
            _gradeService.SetStrategy(new MyRACIT.Strategies.AverageGradeStrategy());
            var averageGrade = await _gradeService.GetAverageGradeForStudentAsync(studentId);

            ViewBag.Student = student;
            ViewBag.AverageGrade = averageGrade;

            return View(grades);
        }
    }
}

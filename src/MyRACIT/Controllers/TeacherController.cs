using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyRACIT.Data;
using MyRACIT.Models.Entities;
using System.Security.Claims;

namespace MyRACIT.Controllers
{
    /// <summary>
    /// Контролер для викладачів (UC8-UC12)
    /// </summary>
    [Authorize(Roles = "Teacher")]
    public class TeacherController : Controller
    {
        private readonly MyRacitDbContext _context;
        
        public TeacherController(MyRacitDbContext context)
        {
            _context = context;
        }
        
        // ===== HELPER: Отримання TeacherProfile поточного користувача =====
        
        private async Task<TeacherProfile?> GetCurrentTeacherProfileAsync()
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            
            return await _context.TeacherProfiles
                .Include(t => t.User)
                .Include(t => t.Department)
                .FirstOrDefaultAsync(t => t.UserId == userId);
        }
        
        // ===== UC8: ПЕРЕГЛЯД СВОЇХ КУРСІВ =====
        
        // GET: Teacher/Index або Teacher/MyCourses
        public async Task<IActionResult> Index()
        {
            var teacher = await GetCurrentTeacherProfileAsync();
            if (teacher == null)
            {
                return NotFound("Профіль викладача не знайдено");
            }
            
            var courses = await _context.Courses
                .Include(c => c.Subject)
                .Include(c => c.Group)
                    .ThenInclude(g => g!.Specialty)
                .Where(c => c.TeacherId == teacher.Id)
                .OrderByDescending(c => c.StartDate)
                .ToListAsync();
            
            ViewBag.TeacherName = teacher.User!.Name;
            
            return View(courses);
        }
        
        // GET: Teacher/CourseDetails/5
        public async Task<IActionResult> CourseDetails(int id)
        {
            var teacher = await GetCurrentTeacherProfileAsync();
            if (teacher == null)
            {
                return NotFound("Профіль викладача не знайдено");
            }
            
            var course = await _context.Courses
                .Include(c => c.Subject)
                .Include(c => c.Group)
                    .ThenInclude(g => g!.Specialty)
                .FirstOrDefaultAsync(c => c.Id == id && c.TeacherId == teacher.Id);
            
            if (course == null)
            {
                return NotFound("Курс не знайдено або у вас немає доступу");
            }
            
            // Завдання курсу
            var assignments = await _context.Assignments
                .Where(a => a.CourseId == id)
                .OrderByDescending(a => a.CreatedAt)
                .ToListAsync();
            
            // Студенти групи
            var students = await _context.StudentProfiles
                .Include(s => s.User)
                .Where(s => s.GroupId == course.GroupId)
                .OrderBy(s => s.User!.Name)
                .ToListAsync();
            
            ViewBag.Assignments = assignments;
            ViewBag.Students = students;
            
            return View(course);
        }
        
        // ===== UC9: СТВОРЕННЯ ЗАВДАННЯ =====
        
        // GET: Teacher/CreateAssignment?courseId=5
        public async Task<IActionResult> CreateAssignment(int courseId)
        {
            var teacher = await GetCurrentTeacherProfileAsync();
            if (teacher == null)
            {
                return NotFound("Профіль викладача не знайдено");
            }
            
            var course = await _context.Courses
                .Include(c => c.Subject)
                .FirstOrDefaultAsync(c => c.Id == courseId && c.TeacherId == teacher.Id);
            
            if (course == null)
            {
                return NotFound("Курс не знайдено");
            }
            
            ViewBag.Course = course;
            
            var assignment = new Assignment
            {
                CourseId = courseId
            };
            
            return View(assignment);
        }
        
        // POST: Teacher/CreateAssignment
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateAssignment(Assignment assignment)
        {
            var teacher = await GetCurrentTeacherProfileAsync();
            if (teacher == null)
            {
                return NotFound("Профіль викладача не знайдено");
            }
            
            // Перевіряємо доступ до курсу
            var course = await _context.Courses
                .FirstOrDefaultAsync(c => c.Id == assignment.CourseId && c.TeacherId == teacher.Id);
            
            if (course == null)
            {
                return NotFound("Курс не знайдено");
            }
            
            if (!ModelState.IsValid)
            {
                ViewBag.Course = await _context.Courses
                    .Include(c => c.Subject)
                    .FirstOrDefaultAsync(c => c.Id == assignment.CourseId);
                return View(assignment);
            }
            
            assignment.CreatedAt = DateTime.Now;
            
            _context.Assignments.Add(assignment);
            await _context.SaveChangesAsync();
            
            TempData["Success"] = $"Завдання '{assignment.Title}' успішно створено!";
            return RedirectToAction(nameof(AssignmentDetails), new { id = assignment.Id });
        }
        
        // GET: Teacher/EditAssignment/5
        public async Task<IActionResult> EditAssignment(int id)
        {
            var teacher = await GetCurrentTeacherProfileAsync();
            if (teacher == null)
            {
                return NotFound("Профіль викладача не знайдено");
            }
            
            var assignment = await _context.Assignments
                .Include(a => a.Course)
                .FirstOrDefaultAsync(a => a.Id == id);
            
            if (assignment == null || assignment.Course!.TeacherId != teacher.Id)
            {
                return NotFound("Завдання не знайдено або у вас немає доступу");
            }
            
            ViewBag.Course = await _context.Courses
                .Include(c => c.Subject)
                .FirstOrDefaultAsync(c => c.Id == assignment.CourseId);
            
            return View(assignment);
        }
        
        // POST: Teacher/EditAssignment/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditAssignment(int id, Assignment assignment)
        {
            if (id != assignment.Id)
            {
                return BadRequest();
            }
            
            var teacher = await GetCurrentTeacherProfileAsync();
            if (teacher == null)
            {
                return NotFound("Профіль викладача не знайдено");
            }
            
            var existingAssignment = await _context.Assignments
                .Include(a => a.Course)
                .FirstOrDefaultAsync(a => a.Id == id);
            
            if (existingAssignment == null || existingAssignment.Course!.TeacherId != teacher.Id)
            {
                return NotFound("Завдання не знайдено");
            }
            
            if (!ModelState.IsValid)
            {
                ViewBag.Course = await _context.Courses
                    .Include(c => c.Subject)
                    .FirstOrDefaultAsync(c => c.Id == assignment.CourseId);
                return View(assignment);
            }
            
            // Оновлюємо тільки потрібні поля
            existingAssignment.Title = assignment.Title;
            existingAssignment.Description = assignment.Description;
            existingAssignment.DueDate = assignment.DueDate;
            existingAssignment.MaxPoints = assignment.MaxPoints;
            
            await _context.SaveChangesAsync();
            
            TempData["Success"] = "Завдання оновлено!";
            return RedirectToAction(nameof(AssignmentDetails), new { id = assignment.Id });
        }
        
        // GET: Teacher/AssignmentDetails/5
        public async Task<IActionResult> AssignmentDetails(int id)
        {
            var teacher = await GetCurrentTeacherProfileAsync();
            if (teacher == null)
            {
                return NotFound("Профіль викладача не знайдено");
            }
            
            var assignment = await _context.Assignments
                .Include(a => a.Course)
                    .ThenInclude(c => c!.Subject)
                .Include(a => a.Course)
                    .ThenInclude(c => c!.Group)
                .FirstOrDefaultAsync(a => a.Id == id);
            
            if (assignment == null || assignment.Course!.TeacherId != teacher.Id)
            {
                return NotFound("Завдання не знайдено");
            }
            
            // Файли завдання
            var files = await _context.AssignmentFiles
                .Where(f => f.AssignmentId == id)
                .ToListAsync();
            
            // Посилання завдання
            var links = await _context.AssignmentLinks
                .Where(l => l.AssignmentId == id)
                .ToListAsync();
            
            // Подані роботи
            var submissions = await _context.Submissions
                .Include(s => s.StudentProfile)
                    .ThenInclude(sp => sp!.User)
                .Include(s => s.Grade)
                .Where(s => s.AssignmentId == id)
                .OrderByDescending(s => s.SubmittedAt)
                .ToListAsync();
            
            ViewBag.Files = files;
            ViewBag.Links = links;
            ViewBag.Submissions = submissions;
            
            // Статистика
            var totalStudents = await _context.StudentProfiles
                .CountAsync(s => s.GroupId == assignment.Course.GroupId);
            ViewBag.TotalStudents = totalStudents;
            ViewBag.SubmittedCount = submissions.Count;
            ViewBag.GradedCount = submissions.Count(s => s.Grade != null);
            
            return View(assignment);
        }
        
        // POST: Teacher/DeleteAssignment/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteAssignment(int id)
        {
            var teacher = await GetCurrentTeacherProfileAsync();
            if (teacher == null)
            {
                return NotFound("Профіль викладача не знайдено");
            }
            
            var assignment = await _context.Assignments
                .Include(a => a.Course)
                .FirstOrDefaultAsync(a => a.Id == id);
            
            if (assignment == null || assignment.Course!.TeacherId != teacher.Id)
            {
                return NotFound("Завдання не знайдено");
            }
            
            // Перевіряємо чи є подані роботи
            var hasSubmissions = await _context.Submissions.AnyAsync(s => s.AssignmentId == id);
            if (hasSubmissions)
            {
                TempData["Error"] = "Неможливо видалити завдання, до якого вже є подані роботи!";
                return RedirectToAction(nameof(AssignmentDetails), new { id });
            }
            
            var courseId = assignment.CourseId;
            
            _context.Assignments.Remove(assignment);
            await _context.SaveChangesAsync();
            
            TempData["Success"] = "Завдання видалено!";
            return RedirectToAction(nameof(CourseDetails), new { id = courseId });
        }
        
        // ===== UC12: ПЕРЕГЛЯД ПОДАНИХ РОБІТ =====
        
        // GET: Teacher/Submissions?assignmentId=5
        public async Task<IActionResult> Submissions(int assignmentId)
        {
            var teacher = await GetCurrentTeacherProfileAsync();
            if (teacher == null)
            {
                return NotFound("Профіль викладача не знайдено");
            }
            
            var assignment = await _context.Assignments
                .Include(a => a.Course)
                    .ThenInclude(c => c!.Subject)
                .FirstOrDefaultAsync(a => a.Id == assignmentId);
            
            if (assignment == null || assignment.Course!.TeacherId != teacher.Id)
            {
                return NotFound("Завдання не знайдено");
            }
            
            var submissions = await _context.Submissions
                .Include(s => s.StudentProfile)
                    .ThenInclude(sp => sp!.User)
                .Include(s => s.Grade)
                .Where(s => s.AssignmentId == assignmentId)
                .OrderByDescending(s => s.SubmittedAt)
                .ToListAsync();
            
            ViewBag.Assignment = assignment;
            
            return View(submissions);
        }
        
        // GET: Teacher/SubmissionDetails/5
        public async Task<IActionResult> SubmissionDetails(int id)
        {
            var teacher = await GetCurrentTeacherProfileAsync();
            if (teacher == null)
            {
                return NotFound("Профіль викладача не знайдено");
            }
            
            var submission = await _context.Submissions
                .Include(s => s.Assignment)
                    .ThenInclude(a => a!.Course)
                .Include(s => s.StudentProfile)
                    .ThenInclude(sp => sp!.User)
                .Include(s => s.Grade)
                .FirstOrDefaultAsync(s => s.Id == id);
            
            if (submission == null || submission.Assignment!.Course!.TeacherId != teacher.Id)
            {
                return NotFound("Роботу не знайдено");
            }
            
            return View(submission);
        }
        
        // ===== UC10: ОЦІНЮВАННЯ РОБІТ =====
        
        // GET: Teacher/GradeSubmission/5
        public async Task<IActionResult> GradeSubmission(int id)
        {
            var teacher = await GetCurrentTeacherProfileAsync();
            if (teacher == null)
            {
                return NotFound("Профіль викладача не знайдено");
            }
            
            var submission = await _context.Submissions
                .Include(s => s.Assignment)
                    .ThenInclude(a => a!.Course)
                .Include(s => s.StudentProfile)
                    .ThenInclude(sp => sp!.User)
                .Include(s => s.Grade)
                .FirstOrDefaultAsync(s => s.Id == id);
            
            if (submission == null || submission.Assignment!.Course!.TeacherId != teacher.Id)
            {
                return NotFound("Роботу не знайдено");
            }
            
            var grade = submission.Grade ?? new Grade
            {
                SubmissionId = submission.Id,
                StudentProfileId = submission.StudentProfileId
            };
            
            ViewBag.Submission = submission;
            ViewBag.MaxPoints = submission.Assignment.MaxPoints;
            
            return View(grade);
        }
        
        // POST: Teacher/GradeSubmission
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> GradeSubmission(Grade grade)
        {
            var teacher = await GetCurrentTeacherProfileAsync();
            if (teacher == null)
            {
                return NotFound("Профіль викладача не знайдено");
            }
            
            var submission = await _context.Submissions
                .Include(s => s.Assignment)
                    .ThenInclude(a => a!.Course)
                .Include(s => s.Grade)
                .FirstOrDefaultAsync(s => s.Id == grade.SubmissionId);
            
            if (submission == null || submission.Assignment!.Course!.TeacherId != teacher.Id)
            {
                return NotFound("Роботу не знайдено");
            }
            
            // Валідація балів
            if (grade.Points < 0 || grade.Points > submission.Assignment.MaxPoints)
            {
                ModelState.AddModelError("Points", 
                    $"Бали мають бути від 0 до {submission.Assignment.MaxPoints}");
            }
            
            if (!ModelState.IsValid)
            {
                ViewBag.Submission = await _context.Submissions
                    .Include(s => s.Assignment)
                    .Include(s => s.StudentProfile)
                        .ThenInclude(sp => sp!.User)
                    .FirstOrDefaultAsync(s => s.Id == grade.SubmissionId);
                ViewBag.MaxPoints = submission.Assignment.MaxPoints;
                return View(grade);
            }
            
            grade.GradedAt = DateTime.Now;
            
            if (submission.Grade == null)
            {
                // Створюємо нову оцінку
                grade.StudentProfileId = submission.StudentProfileId;
                _context.Grades.Add(grade);
            }
            else
            {
                // Оновлюємо існуючу
                submission.Grade.Points = grade.Points;
                submission.Grade.Feedback = grade.Feedback;
                submission.Grade.GradedAt = grade.GradedAt;
            }
            
            await _context.SaveChangesAsync();
            
            TempData["Success"] = $"Роботу оцінено на {grade.Points} балів!";
            return RedirectToAction(nameof(AssignmentDetails), new { id = submission.AssignmentId });
        }
        
        // ===== UC11: ПЕРЕГЛЯД ОЦІНОК КУРСУ =====
        
        // GET: Teacher/Grades?courseId=5
        public async Task<IActionResult> Grades(int courseId)
        {
            var teacher = await GetCurrentTeacherProfileAsync();
            if (teacher == null)
            {
                return NotFound("Профіль викладача не знайдено");
            }
            
            var course = await _context.Courses
                .Include(c => c.Subject)
                .Include(c => c.Group)
                .FirstOrDefaultAsync(c => c.Id == courseId && c.TeacherId == teacher.Id);
            
            if (course == null)
            {
                return NotFound("Курс не знайдено");
            }
            
            // Студенти групи
            var students = await _context.StudentProfiles
                .Include(s => s.User)
                .Where(s => s.GroupId == course.GroupId)
                .OrderBy(s => s.User!.Name)
                .ToListAsync();
            
            // Завдання курсу
            var assignments = await _context.Assignments
                .Where(a => a.CourseId == courseId)
                .OrderBy(a => a.DueDate)
                .ToListAsync();
            
            // Оцінки студентів
            var grades = await _context.Grades
                .Include(g => g.Submission)
                    .ThenInclude(s => s!.Assignment)
                .Where(g => g.Submission!.Assignment!.CourseId == courseId)
                .ToListAsync();
            
            ViewBag.Course = course;
            ViewBag.Students = students;
            ViewBag.Assignments = assignments;
            ViewBag.Grades = grades;
            
            return View();
        }
        
        // GET: Teacher/StudentGrades?courseId=5&studentId=10
        public async Task<IActionResult> StudentGrades(int courseId, int studentId)
        {
            var teacher = await GetCurrentTeacherProfileAsync();
            if (teacher == null)
            {
                return NotFound("Профіль викладача не знайдено");
            }
            
            var course = await _context.Courses
                .Include(c => c.Subject)
                .FirstOrDefaultAsync(c => c.Id == courseId && c.TeacherId == teacher.Id);
            
            if (course == null)
            {
                return NotFound("Курс не знайдено");
            }
            
            var student = await _context.StudentProfiles
                .Include(s => s.User)
                .Include(s => s.Group)
                .FirstOrDefaultAsync(s => s.Id == studentId && s.GroupId == course.GroupId);
            
            if (student == null)
            {
                return NotFound("Студент не знайдено");
            }
            
            // Завдання та оцінки студента
            var assignments = await _context.Assignments
                .Where(a => a.CourseId == courseId)
                .OrderBy(a => a.DueDate)
                .Select(a => new
                {
                    Assignment = a,
                    Submission = _context.Submissions
                        .Include(s => s.Grade)
                        .FirstOrDefault(s => s.AssignmentId == a.Id && s.StudentProfileId == studentId)
                })
                .ToListAsync();
            
            ViewBag.Course = course;
            ViewBag.Student = student;
            ViewBag.Assignments = assignments;
            
            return View();
        }
    }
}

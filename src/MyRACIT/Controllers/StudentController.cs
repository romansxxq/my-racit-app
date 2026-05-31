using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyRACIT.Data;
using MyRACIT.Models.Entities;
using System.Security.Claims;

namespace MyRACIT.Controllers
{
    /// <summary>
    /// Контролер для студентів (UC13-UC17)
    /// </summary>
    [Authorize(Roles = "Student")]
    public class StudentController : Controller
    {
        private readonly MyRacitDbContext _context;
        
        public StudentController(MyRacitDbContext context)
        {
            _context = context;
        }
        
        // ===== HELPER: Отримання StudentProfile поточного користувача =====
        
        private async Task<StudentProfile?> GetCurrentStudentProfileAsync()
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            
            return await _context.StudentProfiles
                .Include(s => s.User)
                .Include(s => s.Group)
                    .ThenInclude(g => g!.Specialty)
                .FirstOrDefaultAsync(s => s.UserId == userId);
        }
        
        // ===== UC13: ПЕРЕГЛЯД ПРОФІЛЮ =====
        
        // GET: Student/Index або Student/Profile
        public async Task<IActionResult> Index()
        {
            var student = await GetCurrentStudentProfileAsync();
            if (student == null)
            {
                return NotFound("Профіль студента не знайдено");
            }
            
            // Статистика студента
            var totalCourses = await _context.Courses
                .CountAsync(c => c.GroupId == student.GroupId);
            
            var totalSubmissions = await _context.Submissions
                .CountAsync(s => s.StudentProfileId == student.Id);
            
            var totalGrades = await _context.Grades
                .CountAsync(g => g.StudentProfileId == student.Id);
            
            var averageGrade = await _context.Grades
                .Where(g => g.StudentProfileId == student.Id)
                .AverageAsync(g => (double?)g.Points) ?? 0;
            
            ViewBag.TotalCourses = totalCourses;
            ViewBag.TotalSubmissions = totalSubmissions;
            ViewBag.TotalGrades = totalGrades;
            ViewBag.AverageGrade = Math.Round(averageGrade, 2);
            
            return View(student);
        }
        
        // ===== UC14: ПЕРЕГЛЯД ДОСТУПНИХ КУРСІВ =====
        
        // GET: Student/MyCourses
        public async Task<IActionResult> MyCourses()
        {
            var student = await GetCurrentStudentProfileAsync();
            if (student == null)
            {
                return NotFound("Профіль студента не знайдено");
            }
            
            // Курси групи студента
            var courses = await _context.Courses
                .Include(c => c.Subject)
                .Include(c => c.Teacher)
                    .ThenInclude(t => t!.User)
                .Include(c => c.Group)
                .Where(c => c.GroupId == student.GroupId)
                .OrderByDescending(c => c.StartDate)
                .ToListAsync();
            
            ViewBag.StudentName = student.User!.Name;
            ViewBag.GroupName = student.Group!.Name;
            
            return View(courses);
        }
        
        // GET: Student/CourseDetails/5
        public async Task<IActionResult> CourseDetails(int id)
        {
            var student = await GetCurrentStudentProfileAsync();
            if (student == null)
            {
                return NotFound("Профіль студента не знайдено");
            }
            
            var course = await _context.Courses
                .Include(c => c.Subject)
                .Include(c => c.Teacher)
                    .ThenInclude(t => t!.User)
                .Include(c => c.Teacher)
                    .ThenInclude(t => t!.Department)
                .Include(c => c.Group)
                .FirstOrDefaultAsync(c => c.Id == id && c.GroupId == student.GroupId);
            
            if (course == null)
            {
                return NotFound("Курс не знайдено або у вас немає доступу");
            }
            
            // Завдання курсу
            var assignments = await _context.Assignments
                .Where(a => a.CourseId == id)
                .OrderByDescending(a => a.DueDate)
                .ToListAsync();
            
            // Мої подані роботи
            var mySubmissions = await _context.Submissions
                .Include(s => s.Grade)
                .Where(s => s.StudentProfileId == student.Id 
                         && s.Assignment!.CourseId == id)
                .ToListAsync();
            
            ViewBag.Assignments = assignments;
            ViewBag.MySubmissions = mySubmissions;
            
            return View(course);
        }
        
        // ===== UC15: ПЕРЕГЛЯД ЗАВДАНЬ КУРСУ =====
        
        // GET: Student/Assignments?courseId=5
        public async Task<IActionResult> Assignments(int courseId)
        {
            var student = await GetCurrentStudentProfileAsync();
            if (student == null)
            {
                return NotFound("Профіль студента не знайдено");
            }
            
            var course = await _context.Courses
                .Include(c => c.Subject)
                .FirstOrDefaultAsync(c => c.Id == courseId && c.GroupId == student.GroupId);
            
            if (course == null)
            {
                return NotFound("Курс не знайдено");
            }
            
            var assignments = await _context.Assignments
                .Where(a => a.CourseId == courseId)
                .OrderByDescending(a => a.DueDate)
                .Select(a => new
                {
                    Assignment = a,
                    MySubmission = _context.Submissions
                        .Include(s => s.Grade)
                        .FirstOrDefault(s => s.AssignmentId == a.Id 
                                          && s.StudentProfileId == student.Id)
                })
                .ToListAsync();
            
            ViewBag.Course = course;
            ViewBag.Assignments = assignments;
            
            return View();
        }
        
        // GET: Student/AssignmentDetails/5
        public async Task<IActionResult> AssignmentDetails(int id)
        {
            var student = await GetCurrentStudentProfileAsync();
            if (student == null)
            {
                return NotFound("Профіль студента не знайдено");
            }
            
            var assignment = await _context.Assignments
                .Include(a => a.Course)
                    .ThenInclude(c => c!.Subject)
                .Include(a => a.Course)
                    .ThenInclude(c => c!.Teacher)
                        .ThenInclude(t => t!.User)
                .FirstOrDefaultAsync(a => a.Id == id);
            
            if (assignment == null || assignment.Course!.GroupId != student.GroupId)
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
            
            // Моя робота (якщо подана)
            var mySubmission = await _context.Submissions
                .Include(s => s.Grade)
                .FirstOrDefaultAsync(s => s.AssignmentId == id 
                                       && s.StudentProfileId == student.Id);
            
            ViewBag.Files = files;
            ViewBag.Links = links;
            ViewBag.MySubmission = mySubmission;
            
            // Статус дедлайну
            var now = DateTime.Now;
            ViewBag.IsOverdue = assignment.DueDate < now;
            ViewBag.DaysUntilDue = (assignment.DueDate - now).Days;
            
            return View(assignment);
        }
        
        // ===== UC16: ПОДАННЯ РОБОТИ =====
        
        // GET: Student/SubmitAssignment?assignmentId=5
        public async Task<IActionResult> SubmitAssignment(int assignmentId)
        {
            var student = await GetCurrentStudentProfileAsync();
            if (student == null)
            {
                return NotFound("Профіль студента не знайдено");
            }
            
            var assignment = await _context.Assignments
                .Include(a => a.Course)
                    .ThenInclude(c => c!.Subject)
                .FirstOrDefaultAsync(a => a.Id == assignmentId);
            
            if (assignment == null || assignment.Course!.GroupId != student.GroupId)
            {
                return NotFound("Завдання не знайдено");
            }
            
            // Перевіряємо чи вже подана робота
            var existingSubmission = await _context.Submissions
                .FirstOrDefaultAsync(s => s.AssignmentId == assignmentId 
                                       && s.StudentProfileId == student.Id);
            
            if (existingSubmission != null)
            {
                TempData["Info"] = "Ви вже подали роботу для цього завдання. Можете переподати.";
            }
            
            ViewBag.Assignment = assignment;
            
            var submission = new Submission
            {
                AssignmentId = assignmentId,
                StudentProfileId = student.Id
            };
            
            return View(submission);
        }
        
        // POST: Student/SubmitAssignment
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SubmitAssignment(Submission submission)
        {
            var student = await GetCurrentStudentProfileAsync();
            if (student == null)
            {
                return NotFound("Профіль студента не знайдено");
            }
            
            var assignment = await _context.Assignments
                .Include(a => a.Course)
                .FirstOrDefaultAsync(a => a.Id == submission.AssignmentId);
            
            if (assignment == null || assignment.Course!.GroupId != student.GroupId)
            {
                return NotFound("Завдання не знайдено");
            }
            
            if (!ModelState.IsValid)
            {
                ViewBag.Assignment = await _context.Assignments
                    .Include(a => a.Course)
                        .ThenInclude(c => c!.Subject)
                    .FirstOrDefaultAsync(a => a.Id == submission.AssignmentId);
                return View(submission);
            }
            
            // Перевіряємо чи вже є подана робота
            var existingSubmission = await _context.Submissions
                .FirstOrDefaultAsync(s => s.AssignmentId == submission.AssignmentId 
                                       && s.StudentProfileId == student.Id);
            
            if (existingSubmission != null)
            {
                // Переподання роботи
                existingSubmission.Content = submission.Content;
                existingSubmission.FilePath = submission.FilePath;
                existingSubmission.SubmittedAt = DateTime.Now;
                
                // Видаляємо стару оцінку (викладач має оцінити заново)
                if (existingSubmission.Grade != null)
                {
                    _context.Grades.Remove(existingSubmission.Grade);
                    TempData["Info"] = "Робота переподана. Попередня оцінка видалена.";
                }
                
                await _context.SaveChangesAsync();
                
                TempData["Success"] = "Робота успішно переподана!";
                return RedirectToAction(nameof(MySubmissions), new { courseId = assignment.CourseId });
            }
            else
            {
                // Нова робота
                submission.StudentProfileId = student.Id;
                submission.SubmittedAt = DateTime.Now;
                
                _context.Submissions.Add(submission);
                await _context.SaveChangesAsync();
                
                TempData["Success"] = "Робота успішно подана!";
                return RedirectToAction(nameof(MySubmissions), new { courseId = assignment.CourseId });
            }
        }
        
        // GET: Student/MySubmissions?courseId=5
        public async Task<IActionResult> MySubmissions(int courseId)
        {
            var student = await GetCurrentStudentProfileAsync();
            if (student == null)
            {
                return NotFound("Профіль студента не знайдено");
            }
            
            var course = await _context.Courses
                .Include(c => c.Subject)
                .FirstOrDefaultAsync(c => c.Id == courseId && c.GroupId == student.GroupId);
            
            if (course == null)
            {
                return NotFound("Курс не знайдено");
            }
            
            var submissions = await _context.Submissions
                .Include(s => s.Assignment)
                .Include(s => s.Grade)
                .Where(s => s.StudentProfileId == student.Id 
                         && s.Assignment!.CourseId == courseId)
                .OrderByDescending(s => s.SubmittedAt)
                .ToListAsync();
            
            ViewBag.Course = course;
            
            return View(submissions);
        }
        
        // GET: Student/SubmissionDetails/5
        public async Task<IActionResult> SubmissionDetails(int id)
        {
            var student = await GetCurrentStudentProfileAsync();
            if (student == null)
            {
                return NotFound("Профіль студента не знайдено");
            }
            
            var submission = await _context.Submissions
                .Include(s => s.Assignment)
                    .ThenInclude(a => a!.Course)
                        .ThenInclude(c => c!.Subject)
                .Include(s => s.Grade)
                .FirstOrDefaultAsync(s => s.Id == id && s.StudentProfileId == student.Id);
            
            if (submission == null)
            {
                return NotFound("Роботу не знайдено");
            }
            
            return View(submission);
        }
        
        // POST: Student/DeleteSubmission/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteSubmission(int id)
        {
            var student = await GetCurrentStudentProfileAsync();
            if (student == null)
            {
                return NotFound("Профіль студента не знайдено");
            }
            
            var submission = await _context.Submissions
                .Include(s => s.Assignment)
                .Include(s => s.Grade)
                .FirstOrDefaultAsync(s => s.Id == id && s.StudentProfileId == student.Id);
            
            if (submission == null)
            {
                return NotFound("Роботу не знайдено");
            }
            
            // Не можна видалити оцінену роботу
            if (submission.Grade != null)
            {
                TempData["Error"] = "Неможливо видалити роботу, яку вже оцінено!";
                return RedirectToAction(nameof(SubmissionDetails), new { id });
            }
            
            var courseId = submission.Assignment!.CourseId;
            
            _context.Submissions.Remove(submission);
            await _context.SaveChangesAsync();
            
            TempData["Success"] = "Роботу видалено!";
            return RedirectToAction(nameof(MySubmissions), new { courseId });
        }
        
        // ===== UC17: ПЕРЕГЛЯД СВОЇХ ОЦІНОК =====
        
        // GET: Student/MyGrades
        public async Task<IActionResult> MyGrades()
        {
            var student = await GetCurrentStudentProfileAsync();
            if (student == null)
            {
                return NotFound("Профіль студента не знайдено");
            }
            
            // Всі оцінки студента згруповані по курсах
            var grades = await _context.Grades
                .Include(g => g.Submission)
                    .ThenInclude(s => s!.Assignment)
                        .ThenInclude(a => a!.Course)
                            .ThenInclude(c => c!.Subject)
                .Where(g => g.StudentProfileId == student.Id)
                .OrderByDescending(g => g.GradedAt)
                .ToListAsync();
            
            // Групуємо по курсах
            var gradesByCourse = grades
                .GroupBy(g => g.Submission!.Assignment!.Course)
                .ToList();
            
            ViewBag.GradesByCourse = gradesByCourse;
            
            // Загальна статистика
            var totalGrades = grades.Count;
            var averageGrade = grades.Any() 
                ? Math.Round(grades.Average(g => g.Points), 2) 
                : 0;
            var totalPoints = grades.Sum(g => g.Points);
            var maxPossiblePoints = grades.Sum(g => g.Submission!.Assignment!.MaxPoints);
            
            ViewBag.TotalGrades = totalGrades;
            ViewBag.AverageGrade = averageGrade;
            ViewBag.TotalPoints = totalPoints;
            ViewBag.MaxPossiblePoints = maxPossiblePoints;
            ViewBag.Percentage = maxPossiblePoints > 0 
                ? Math.Round((double)totalPoints / maxPossiblePoints * 100, 2) 
                : 0;
            
            return View(student);
        }
        
        // GET: Student/CourseGrades?courseId=5
        public async Task<IActionResult> CourseGrades(int courseId)
        {
            var student = await GetCurrentStudentProfileAsync();
            if (student == null)
            {
                return NotFound("Профіль студента не знайдено");
            }
            
            var course = await _context.Courses
                .Include(c => c.Subject)
                .Include(c => c.Teacher)
                    .ThenInclude(t => t!.User)
                .FirstOrDefaultAsync(c => c.Id == courseId && c.GroupId == student.GroupId);
            
            if (course == null)
            {
                return NotFound("Курс не знайдено");
            }
            
            // Завдання курсу з моїми оцінками
            var assignments = await _context.Assignments
                .Where(a => a.CourseId == courseId)
                .OrderBy(a => a.DueDate)
                .Select(a => new
                {
                    Assignment = a,
                    MySubmission = _context.Submissions
                        .Include(s => s.Grade)
                        .FirstOrDefault(s => s.AssignmentId == a.Id 
                                          && s.StudentProfileId == student.Id)
                })
                .ToListAsync();
            
            ViewBag.Course = course;
            ViewBag.Assignments = assignments;
            
            // Статистика по курсу
            var grades = assignments
                .Where(a => a.MySubmission?.Grade != null)
                .Select(a => a.MySubmission!.Grade!)
                .ToList();
            
            ViewBag.TotalAssignments = assignments.Count;
            ViewBag.CompletedAssignments = assignments.Count(a => a.MySubmission?.Grade != null);
            ViewBag.PendingAssignments = assignments.Count(a => a.MySubmission != null && a.MySubmission.Grade == null);
            ViewBag.NotSubmittedAssignments = assignments.Count(a => a.MySubmission == null);
            
            if (grades.Any())
            {
                var totalPoints = grades.Sum(g => g.Points);
                var maxPossiblePoints = grades.Sum(g => assignments
                    .First(a => a.MySubmission?.Grade?.Id == g.Id)
                    .Assignment.MaxPoints);
                
                ViewBag.TotalPoints = totalPoints;
                ViewBag.MaxPoints = maxPossiblePoints;
                ViewBag.CourseAverage = Math.Round((double)totalPoints / maxPossiblePoints * 100, 2);
            }
            else
            {
                ViewBag.TotalPoints = 0;
                ViewBag.MaxPoints = 0;
                ViewBag.CourseAverage = 0;
            }
            
            return View();
        }
    }
}

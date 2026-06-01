using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyRACIT.Data;
using MyRACIT.Models.Entities;
using MyRACIT.Services.FileValidation;
using MyRACIT.Services.Interfaces;
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
        private readonly IFileStorageService _fileStorage;
        private readonly IFileValidationStrategy _fileValidator = new AssignmentFileValidator();

        public TeacherController(MyRacitDbContext context, IFileStorageService fileStorage)
        {
            _context = context;
            _fileStorage = fileStorage;
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
        
        //Перегляд своїх курсів
        
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

            var assignmentCounts = new Dictionary<int, int>();
            var submissionCounts = new Dictionary<int, int>();
            var studentCounts = new Dictionary<int, int>();

            if (courses.Count > 0)
            {
                var courseIds = courses.Select(c => c.Id).ToList();
                var groupIds = courses.Select(c => c.GroupId).Distinct().ToList();

                assignmentCounts = await _context.Assignments
                    .Where(a => courseIds.Contains(a.CourseId))
                    .GroupBy(a => a.CourseId)
                    .Select(g => new { CourseId = g.Key, Count = g.Count() })
                    .ToDictionaryAsync(x => x.CourseId, x => x.Count);

                submissionCounts = await _context.Submissions
                    .Join(
                        _context.Assignments,
                        s => s.AssignmentId,
                        a => a.Id,
                        (s, a) => a.CourseId)
                    .Where(courseId => courseIds.Contains(courseId))
                    .GroupBy(courseId => courseId)
                    .Select(g => new { CourseId = g.Key, Count = g.Count() })
                    .ToDictionaryAsync(x => x.CourseId, x => x.Count);

                studentCounts = await _context.StudentProfiles
                    .Where(s => groupIds.Contains(s.GroupId))
                    .GroupBy(s => s.GroupId)
                    .Select(g => new { GroupId = g.Key, Count = g.Count() })
                    .ToDictionaryAsync(x => x.GroupId, x => x.Count);
            }

            ViewBag.GetCourseAssignments = (Func<int, int>)(courseId =>
                assignmentCounts.TryGetValue(courseId, out var count) ? count : 0);

            ViewBag.GetCourseSubmissions = (Func<int, int>)(courseId =>
                submissionCounts.TryGetValue(courseId, out var count) ? count : 0);

            ViewBag.GetStudentsCount = (Func<int, int>)(groupId =>
                studentCounts.TryGetValue(groupId, out var count) ? count : 0);
            
            ViewBag.TeacherName = teacher.User?.Name ?? "Викладач";
            
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
            
            // Підрахунок здач та оцінок
            var assignmentIds = assignments.Select(a => a.Id).ToList();
            var submissionsCount = await _context.Submissions
                .CountAsync(s => assignmentIds.Contains(s.AssignmentId));
            var gradesCount = await _context.Grades
                .CountAsync(g => g.Submission!.Assignment!.CourseId == id);

            ViewBag.Assignments = assignments;
            ViewBag.Students = students;
            ViewBag.StudentsCount = students.Count;
            ViewBag.AssignmentsCount = assignments.Count;
            ViewBag.SubmissionsCount = submissionsCount;
            ViewBag.GradesCount = gradesCount;
            
            return View(course);
        }
        
        // GET: Teacher/Assignments?courseId=5
        public async Task<IActionResult> Assignments(int courseId)
        {
            var teacher = await GetCurrentTeacherProfileAsync();
            if (teacher == null)
                return NotFound("Профіль викладача не знайдено");

            var course = await _context.Courses
                .Include(c => c.Subject)
                .Include(c => c.Group)
                .FirstOrDefaultAsync(c => c.Id == courseId && c.TeacherId == teacher.Id);

            if (course == null)
                return NotFound("Курс не знайдено або у вас немає доступу");

            var assignments = await _context.Assignments
                .Where(a => a.CourseId == courseId)
                .OrderByDescending(a => a.CreatedAt)
                .ToListAsync();

            var assignmentIds = assignments.Select(a => a.Id).ToList();

            var submissionCounts = await _context.Submissions
                .Where(s => assignmentIds.Contains(s.AssignmentId))
                .GroupBy(s => s.AssignmentId)
                .Select(g => new { AssignmentId = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.AssignmentId, x => x.Count);

            var studentCount = await _context.StudentProfiles
                .CountAsync(s => s.GroupId == course.GroupId);

            ViewBag.CourseId = courseId;
            ViewBag.CourseName = $"{course.Subject?.Title} ({course.Group?.Name})";
            ViewBag.GetSubmissionsCount = (Func<int, int>)(assignmentId =>
                submissionCounts.TryGetValue(assignmentId, out var cnt) ? cnt : 0);
            ViewBag.GetStudentsCount = (Func<int?, int>)(groupId => studentCount);

            return View(assignments);
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
            ViewBag.CourseId = courseId;
            ViewBag.CourseName = $"{course.Subject?.Title} ({course.Group?.Name})";

            var assignment = new Assignment
            {
                CourseId = courseId
            };
            
            return View(assignment);
        }
        
        // POST: Teacher/CreateAssignment
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateAssignment(Assignment assignment, List<IFormFile>? files)
        {
            var teacher = await GetCurrentTeacherProfileAsync();
            if (teacher == null)
                return NotFound("Профіль викладача не знайдено");

            var course = await _context.Courses
                .Include(c => c.Subject)
                .Include(c => c.Group)
                .FirstOrDefaultAsync(c => c.Id == assignment.CourseId && c.TeacherId == teacher.Id);

            if (course == null)
                return NotFound("Курс не знайдено");

            // Validate uploaded file extensions before ModelState check (Strategy Pattern)
            if (files != null)
            {
                foreach (var file in files)
                {
                    if (!_fileValidator.IsValid(file, out var error))
                        ModelState.AddModelError("files", error);
                }
            }

            if (!ModelState.IsValid)
            {
                ViewBag.Course = course;
                ViewBag.CourseId = assignment.CourseId;
                ViewBag.CourseName = $"{course.Subject?.Title} ({course.Group?.Name})";
                return View(assignment);
            }

            assignment.CreatedAt = DateTime.Now;
            _context.Assignments.Add(assignment);
            await _context.SaveChangesAsync();

            // Save attached files
            if (files != null && files.Count > 0)
            {
                foreach (var file in files)
                {
                    var savedPath = await _fileStorage.SaveFileAsync(file, "assignments");
                    _context.AssignmentFiles.Add(new AssignmentFile
                    {
                        AssignmentId = assignment.Id,
                        FilePath = savedPath,
                        FileName = file.FileName,
                        UploadedAt = DateTime.UtcNow
                    });
                }
                await _context.SaveChangesAsync();
            }

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
                .Include(s => s.Student)
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
                .Include(s => s.Student)
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
                .Include(s => s.Student)
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
                .Include(s => s.Student)
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
                GradedBy = teacher.Id
            };
            
            ViewBag.Grade = grade;
            ViewBag.MaxPoints = submission.Assignment.MaxPoints;
            
            return View(submission);
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
            if (grade.Value < 0 || grade.Value > submission.Assignment.MaxPoints)
            {
                ModelState.AddModelError("Value", 
                    $"Бали мають бути від 0 до {submission.Assignment.MaxPoints}");
            }
            
            if (!ModelState.IsValid)
            {
                var submissionForView = await _context.Submissions
                    .Include(s => s.Assignment)
                        .ThenInclude(a => a!.Course)
                    .Include(s => s.Student)
                        .ThenInclude(sp => sp!.User)
                    .Include(s => s.Grade)
                    .FirstOrDefaultAsync(s => s.Id == grade.SubmissionId);
                ViewBag.Grade = grade;
                ViewBag.MaxPoints = submission.Assignment.MaxPoints;
                return View(submissionForView);
            }
            
            if (submission.Grade == null)
            {
                // Створюємо нову оцінку (явний об'єкт для уникнення EF Core 10 IDENTITY_INSERT помилки)
                var newGrade = new Grade
                {
                    SubmissionId = grade.SubmissionId,
                    Value = grade.Value,
                    Feedback = grade.Feedback,
                    DateIssued = DateTime.Now,
                    GradedBy = teacher.Id
                };
                _context.Grades.Add(newGrade);
            }
            else
            {
                // Оновлюємо існуючу
                submission.Grade.Value = grade.Value;
                submission.Grade.Feedback = grade.Feedback;
                submission.Grade.DateIssued = DateTime.Now;
                submission.Grade.GradedBy = teacher.Id;
            }
            
            await _context.SaveChangesAsync();
            
            TempData["Success"] = $"Роботу оцінено на {grade.Value} балів!";
            return RedirectToAction(nameof(Submissions), new { assignmentId = submission.AssignmentId });
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
                .OrderBy(a => a.Deadline)
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
            
            return View(grades);
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
            var assignmentList = await _context.Assignments
                .Where(a => a.CourseId == courseId)
                .OrderBy(a => a.Deadline)
                .ToListAsync();

            var aIds = assignmentList.Select(a => a.Id).ToList();
            var subsByAssignment = await _context.Submissions
                .Include(s => s.Grade)
                .Where(s => s.StudentId == studentId && aIds.Contains(s.AssignmentId))
                .ToDictionaryAsync(s => s.AssignmentId);

            ViewBag.Course = course;
            ViewBag.Student = student;
            ViewBag.Assignments = assignmentList.Select(a => new
            {
                Assignment = a,
                Submission = subsByAssignment.TryGetValue(a.Id, out var sub) ? sub : (Submission?)null
            }).ToList();
            
            return View();
        }
    }
}

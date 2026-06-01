using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyRACIT.Data;
using MyRACIT.Models.DTOs;
using MyRACIT.Models.Entities;
using MyRACIT.Services.Interfaces;

namespace MyRACIT.Controllers
{
    /// <summary>
    /// Контролер для адміністрування системи (UC1-UC7)
    /// </summary>
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly IUserService _userService;
        private readonly MyRacitDbContext _context;
        
        public AdminController(IUserService userService, MyRacitDbContext context)
        {
            _userService = userService;
            _context = context;
        }
        
        public async Task<IActionResult> Index()
        {
            ViewBag.TotalStudents = await _context.StudentProfiles.CountAsync();
            ViewBag.TotalTeachers = await _context.TeacherProfiles.CountAsync();
            ViewBag.TotalCourses = await _context.Courses.CountAsync();
            ViewBag.TotalGroups = await _context.Groups.CountAsync();
            ViewBag.TotalDepartments = await _context.Departments.CountAsync();
            
            return View();
        }
        
        // GET: Admin/Departments
        public async Task<IActionResult> Departments()
        {
            var departments = await _context.Departments.ToListAsync();
            return View(departments);
        }
        
        // GET: Admin/CreateDepartment
        public IActionResult CreateDepartment()
        {
            return View();
        }
        
        // POST: Admin/CreateDepartment
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateDepartment(Department department)
        {
            if (!ModelState.IsValid)
            {
                return View(department);
            }
            
            _context.Departments.Add(department);
            await _context.SaveChangesAsync();
            
            TempData["Success"] = $"Кафедру '{department.Name}' успішно створено!";
            return RedirectToAction(nameof(Departments));
        }
        
        // GET: Admin/EditDepartment/5
        public async Task<IActionResult> EditDepartment(int id)
        {
            var department = await _context.Departments.FindAsync(id);
            if (department == null)
            {
                return NotFound();
            }
            
            return View(department);
        }
        
        // POST: Admin/EditDepartment/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditDepartment(int id, Department department)
        {
            if (id != department.Id)
            {
                return BadRequest();
            }
            
            if (!ModelState.IsValid)
            {
                return View(department);
            }
            
            _context.Update(department);
            await _context.SaveChangesAsync();
            
            TempData["Success"] = "Кафедру оновлено!";
            return RedirectToAction(nameof(Departments));
        }
        
        // POST: Admin/DeleteDepartment/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteDepartment(int id)
        {
            var department = await _context.Departments.FindAsync(id);
            if (department == null)
            {
                return NotFound();
            }
            
            // Перевіряємо чи немає прив'язаних викладачів
            var hasTeachers = await _context.TeacherProfiles.AnyAsync(t => t.DepartmentId == id);
            if (hasTeachers)
            {
                TempData["Error"] = "Неможливо видалити кафедру, до якої прив'язані викладачі!";
                return RedirectToAction(nameof(Departments));
            }
            
            _context.Departments.Remove(department);
            await _context.SaveChangesAsync();
            
            TempData["Success"] = "Кафедру видалено!";
            return RedirectToAction(nameof(Departments));
        }
        
        // GET: Admin/Specialties
        public async Task<IActionResult> Specialties()
        {
            var specialties = await _context.Specialties.ToListAsync();
            return View(specialties);
        }
        
        // GET: Admin/CreateSpecialty
        public IActionResult CreateSpecialty()
        {
            return View();
        }
        
        // POST: Admin/CreateSpecialty
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateSpecialty(Specialty specialty)
        {
            if (!ModelState.IsValid)
            {
                return View(specialty);
            }
            
            _context.Specialties.Add(specialty);
            await _context.SaveChangesAsync();
            
            TempData["Success"] = $"Спеціальність '{specialty.Name}' створено!";
            return RedirectToAction(nameof(Specialties));
        }
        
        // GET: Admin/EditSpecialty/5
        public async Task<IActionResult> EditSpecialty(int id)
        {
            var specialty = await _context.Specialties.FindAsync(id);
            if (specialty == null)
            {
                return NotFound();
            }
            
            return View(specialty);
        }
        
        // POST: Admin/EditSpecialty/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditSpecialty(int id, Specialty specialty)
        {
            if (id != specialty.Id)
            {
                return BadRequest();
            }
            
            if (!ModelState.IsValid)
            {
                return View(specialty);
            }
            
            _context.Update(specialty);
            await _context.SaveChangesAsync();
            
            TempData["Success"] = "Спеціальність оновлено!";
            return RedirectToAction(nameof(Specialties));
        }
        
        // POST: Admin/DeleteSpecialty/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteSpecialty(int id)
        {
            var specialty = await _context.Specialties.FindAsync(id);
            if (specialty == null)
            {
                return NotFound();
            }
            
            // Перевіряємо чи немає прив'язаних груп
            var hasGroups = await _context.Groups.AnyAsync(g => g.SpecialtyId == id);
            if (hasGroups)
            {
                TempData["Error"] = "Неможливо видалити спеціальність, до якої прив'язані групи!";
                return RedirectToAction(nameof(Specialties));
            }
            
            _context.Specialties.Remove(specialty);
            await _context.SaveChangesAsync();
            
            TempData["Success"] = "Спеціальність видалено!";
            return RedirectToAction(nameof(Specialties));
        }
        
        // ===== UC3: КЕРУВАННЯ ГРУПАМИ =====
        
        // GET: Admin/Groups
        public async Task<IActionResult> Groups()
        {
            var groups = await _context.Groups
                .Include(g => g.Specialty)
                .ToListAsync();
            return View(groups);
        }
        
        // GET: Admin/CreateGroup
        public async Task<IActionResult> CreateGroup()
        {
            ViewBag.Specialties = await _context.Specialties.ToListAsync();
            return View();
        }
        
        // POST: Admin/CreateGroup
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateGroup(Group group)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Specialties = await _context.Specialties.ToListAsync();
                return View(group);
            }
            
            // Перевіряємо унікальність назви групи
            var exists = await _context.Groups.AnyAsync(g => g.Name == group.Name);
            if (exists)
            {
                ModelState.AddModelError("Name", "Група з такою назвою вже існує!");
                ViewBag.Specialties = await _context.Specialties.ToListAsync();
                return View(group);
            }
            
            _context.Groups.Add(group);
            await _context.SaveChangesAsync();
            
            TempData["Success"] = $"Групу '{group.Name}' створено!";
            return RedirectToAction(nameof(Groups));
        }
        
        // GET: Admin/EditGroup/5
        public async Task<IActionResult> EditGroup(int id)
        {
            var group = await _context.Groups.FindAsync(id);
            if (group == null)
            {
                return NotFound();
            }
            
            ViewBag.Specialties = await _context.Specialties.ToListAsync();
            return View(group);
        }
        
        // POST: Admin/EditGroup/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditGroup(int id, Group group)
        {
            if (id != group.Id)
            {
                return BadRequest();
            }
            
            if (!ModelState.IsValid)
            {
                ViewBag.Specialties = await _context.Specialties.ToListAsync();
                return View(group);
            }
            
            _context.Update(group);
            await _context.SaveChangesAsync();
            
            TempData["Success"] = "Групу оновлено!";
            return RedirectToAction(nameof(Groups));
        }
        
        // POST: Admin/DeleteGroup/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteGroup(int id)
        {
            var group = await _context.Groups.FindAsync(id);
            if (group == null)
            {
                return NotFound();
            }
            
            // Перевіряємо чи немає студентів
            var hasStudents = await _context.StudentProfiles.AnyAsync(s => s.GroupId == id);
            if (hasStudents)
            {
                TempData["Error"] = "Неможливо видалити групу, в якій є студенти!";
                return RedirectToAction(nameof(Groups));
            }
            
            _context.Groups.Remove(group);
            await _context.SaveChangesAsync();
            
            TempData["Success"] = "Групу видалено!";
            return RedirectToAction(nameof(Groups));
        }
        
        // ===== UC4: КЕРУВАННЯ ДИСЦИПЛІНАМИ =====
        
        // GET: Admin/Subjects
        public async Task<IActionResult> Subjects()
        {
            var subjects = await _context.Subjects.ToListAsync();
            return View(subjects);
        }
        
        // GET: Admin/CreateSubject
        public IActionResult CreateSubject()
        {
            return View();
        }
        
        // POST: Admin/CreateSubject
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateSubject(Subject subject)
        {
            if (!ModelState.IsValid)
            {
                return View(subject);
            }
            
            _context.Subjects.Add(subject);
            await _context.SaveChangesAsync();
            
            TempData["Success"] = $"Дисципліну '{subject.Title}' створено!";
            return RedirectToAction(nameof(Subjects));
        }
        
        // GET: Admin/EditSubject/5
        public async Task<IActionResult> EditSubject(int id)
        {
            var subject = await _context.Subjects.FindAsync(id);
            if (subject == null)
            {
                return NotFound();
            }
            
            return View(subject);
        }
        
        // POST: Admin/EditSubject/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditSubject(int id, Subject subject)
        {
            if (id != subject.Id)
            {
                return BadRequest();
            }
            
            if (!ModelState.IsValid)
            {
                return View(subject);
            }
            
            _context.Update(subject);
            await _context.SaveChangesAsync();
            
            TempData["Success"] = "Дисципліну оновлено!";
            return RedirectToAction(nameof(Subjects));
        }
        
        // POST: Admin/DeleteSubject/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteSubject(int id)
        {
            var subject = await _context.Subjects.FindAsync(id);
            if (subject == null)
            {
                return NotFound();
            }
            
            // Перевіряємо чи немає курсів
            var hasCourses = await _context.Courses.AnyAsync(c => c.SubjectId == id);
            if (hasCourses)
            {
                TempData["Error"] = "Неможливо видалити дисципліну, яка використовується в курсах!";
                return RedirectToAction(nameof(Subjects));
            }
            
            _context.Subjects.Remove(subject);
            await _context.SaveChangesAsync();
            
            TempData["Success"] = "Дисципліну видалено!";
            return RedirectToAction(nameof(Subjects));
        }
        
        // ===== UC5: КЕРУВАННЯ СТУДЕНТАМИ =====
        
        // GET: Admin/Students
        public async Task<IActionResult> Students()
        {
            var students = await _context.StudentProfiles
                .Include(s => s.User)
                .Include(s => s.Group)
                    .ThenInclude(g => g!.Specialty)
                .ToListAsync();
            return View(students);
        }
        
        // GET: Admin/CreateStudent
        public async Task<IActionResult> CreateStudent()
        {
            ViewBag.Groups = await _context.Groups
                .Include(g => g.Specialty)
                .ToListAsync();
            return View();
        }
        
        // POST: Admin/CreateStudent
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateStudent(CreateStudentDto dto)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Groups = await _context.Groups.Include(g => g.Specialty).ToListAsync();
                return View(dto);
            }
            
            try
            {
                var student = await _userService.CreateStudentAsync(
                    dto.Name, dto.Email, dto.Password, dto.GroupId
                );
                
                TempData["Success"] = $"Студента '{dto.Name}' успішно створено!";
                return RedirectToAction(nameof(Students));
            }
            catch (InvalidOperationException ex)
            {
                ModelState.AddModelError("", ex.Message);
                ViewBag.Groups = await _context.Groups.Include(g => g.Specialty).ToListAsync();
                return View(dto);
            }
        }
        
        // GET: Admin/StudentDetails/5
        public async Task<IActionResult> StudentDetails(int id)
        {
            var student = await _context.StudentProfiles
                .Include(s => s.User)
                .Include(s => s.Group)
                    .ThenInclude(g => g!.Specialty)
                .FirstOrDefaultAsync(s => s.Id == id);
            
            if (student == null)
            {
                return NotFound();
            }
            
            // Отримуємо курси студента через групу
            var courses = await _context.Courses
                .Include(c => c.Subject)
                .Include(c => c.Teacher)
                    .ThenInclude(t => t!.User)
                .Where(c => c.GroupId == student.GroupId)
                .ToListAsync();
            
            ViewBag.Courses = courses;

            // Статистика студента
            var studentSubmissions = await _context.Submissions
                .Include(s => s.Grade)
                .Where(s => s.StudentId == student.Id)
                .ToListAsync();

            var grades = studentSubmissions
                .Where(s => s.Grade != null)
                .Select(s => s.Grade!.Value)
                .ToList();

            ViewBag.TotalSubmissions = studentSubmissions.Count;
            ViewBag.TotalGrades = grades.Count;
            ViewBag.AverageGrade = grades.Count > 0 ? Math.Round(grades.Average(), 1) : (double?)null;
            
            return View(student);
        }       
        
        // GET: Admin/Teachers
        public async Task<IActionResult> Teachers()
        {
            var teachers = await _context.TeacherProfiles
                .Include(t => t.User)
                .Include(t => t.Department)
                .ToListAsync();
            return View(teachers);
        }
        
        // GET: Admin/CreateTeacher
        public async Task<IActionResult> CreateTeacher()
        {
            ViewBag.Departments = await _context.Departments.ToListAsync();
            return View();
        }
        
        // POST: Admin/CreateTeacher
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateTeacher(CreateTeacherDto dto)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Departments = await _context.Departments.ToListAsync();
                return View(dto);
            }
            
            try
            {
                var teacher = await _userService.CreateTeacherAsync(
                    dto.Name, dto.Email, dto.Password, dto.DepartmentId
                );
                
                TempData["Success"] = $"Викладача '{dto.Name}' успішно створено!";
                return RedirectToAction(nameof(Teachers));
            }
            catch (InvalidOperationException ex)
            {
                ModelState.AddModelError("", ex.Message);
                ViewBag.Departments = await _context.Departments.ToListAsync();
                return View(dto);
            }
        }
        
        // GET: Admin/TeacherDetails/5
        public async Task<IActionResult> TeacherDetails(int id)
        {
            var teacher = await _context.TeacherProfiles
                .Include(t => t.User)
                .Include(t => t.Department)
                .FirstOrDefaultAsync(t => t.Id == id);
            
            if (teacher == null)
            {
                return NotFound();
            }
            
            // Отримуємо курси викладача
            var courses = await _context.Courses
                .Include(c => c.Subject)
                .Include(c => c.Group)
                .Where(c => c.TeacherId == id)
                .ToListAsync();
            
            ViewBag.Courses = courses;

            // Статистика викладача
            var courseIds = courses.Select(c => c.Id).ToList();
            var assignments = await _context.Assignments
                .Where(a => courseIds.Contains(a.CourseId))
                .ToListAsync();
            var assignmentIds = assignments.Select(a => a.Id).ToList();
            var gradesGiven = await _context.Grades
                .Where(g => g.GradedBy == teacher.Id)
                .CountAsync();

            ViewBag.TotalCourses = courses.Count;
            ViewBag.TotalAssignments = assignments.Count;
            ViewBag.TotalGradesGiven = gradesGiven;
            
            return View(teacher);
        }
        
        // GET: Admin/Courses
        public async Task<IActionResult> Courses()
        {
            var courses = await _context.Courses
                .Include(c => c.Subject)
                .Include(c => c.Teacher)
                    .ThenInclude(t => t!.User)
                .Include(c => c.Group)
                    .ThenInclude(g => g!.Specialty)
                .ToListAsync();
            return View(courses);
        }
        
        // GET: Admin/CreateCourse
        public async Task<IActionResult> CreateCourse()
        {
            ViewBag.Subjects = await _context.Subjects.ToListAsync();
            ViewBag.Teachers = await _context.TeacherProfiles
                .Include(t => t.User)
                .Include(t => t.Department)
                .ToListAsync();
            ViewBag.Groups = await _context.Groups
                .Include(g => g.Specialty)
                .ToListAsync();
            
            return View();
        }
        
        // POST: Admin/CreateCourse
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateCourse(Course course)
        {
            if (!ModelState.IsValid)
            {
                await LoadCourseViewBags();
                return View(course);
            }
            
            _context.Courses.Add(course);
            await _context.SaveChangesAsync();
            
            TempData["Success"] = "Курс успішно створено!";
            return RedirectToAction(nameof(Courses));
        }
        
        // GET: Admin/CourseDetails/5
        public async Task<IActionResult> CourseDetails(int id)
        {
            var course = await _context.Courses
                .Include(c => c.Subject)
                .Include(c => c.Teacher)
                    .ThenInclude(t => t!.User)
                .Include(c => c.Teacher)
                    .ThenInclude(t => t!.Department)
                .Include(c => c.Group)
                    .ThenInclude(g => g!.Specialty)
                .FirstOrDefaultAsync(c => c.Id == id);
            
            if (course == null)
            {
                return NotFound();
            }
            
            // Студенти групи (автоматично записані на курс)
            var students = await _context.StudentProfiles
                .Include(s => s.User)
                .Where(s => s.GroupId == course.GroupId)
                .ToListAsync();
            
            // Завдання курсу
            var assignments = await _context.Assignments
                .Where(a => a.CourseId == id)
                .ToListAsync();
            
            // Здачі по завданнях цього курсу
            var assignmentIds = assignments.Select(a => a.Id).ToList();
            var submissionsCount = await _context.Submissions
                .Where(s => assignmentIds.Contains(s.AssignmentId))
                .CountAsync();

            ViewBag.Students = students;
            ViewBag.Assignments = assignments;
            ViewBag.StudentsCount = students.Count;
            ViewBag.TotalAssignments = assignments.Count;
            ViewBag.TotalSubmissions = submissionsCount;
            
            return View(course);
        }
        
        // POST: Admin/DeleteCourse/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteCourse(int id)
        {
            var course = await _context.Courses.FindAsync(id);
            if (course == null)
            {
                return NotFound();
            }
            
            // Перевіряємо чи немає завдань
            var hasAssignments = await _context.Assignments.AnyAsync(a => a.CourseId == id);
            if (hasAssignments)
            {
                TempData["Error"] = "Неможливо видалити курс, який має завдання!";
                return RedirectToAction(nameof(Courses));
            }
            
            _context.Courses.Remove(course);
            await _context.SaveChangesAsync();
            
            TempData["Success"] = "Курс видалено!";
            return RedirectToAction(nameof(Courses));
        }
        
        // ===== HELPER METHODS =====
        
        private async Task LoadCourseViewBags()
        {
            ViewBag.Subjects = await _context.Subjects.ToListAsync();
            ViewBag.Teachers = await _context.TeacherProfiles
                .Include(t => t.User)
                .Include(t => t.Department)
                .ToListAsync();
            ViewBag.Groups = await _context.Groups
                .Include(g => g.Specialty)
                .ToListAsync();
        }
    }
}
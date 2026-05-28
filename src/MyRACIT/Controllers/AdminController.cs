using Microsoft.AspNetCore.Mvc;
using MyRACIT.Data;
using MyRACIT.Models.Entities;
using MyRACIT.Factories;
using Microsoft.EntityFrameworkCore;

namespace MyRACIT.Controllers
{
    /// <summary>
    /// Контролер для адміністратора
    /// Управління користувачами через Factory Method Pattern
    /// </summary>
    public class AdminController : Controller
    {
        private readonly IRepository<User> _userRepository;
        private readonly IRepository<StudentProfile> _studentRepository;
        private readonly IRepository<TeacherProfile> _teacherRepository;
        private readonly IRepository<Group> _groupRepository;
        private readonly IRepository<Department> _departmentRepository;

        public AdminController(
            IRepository<User> userRepository,
            IRepository<StudentProfile> studentRepository,
            IRepository<TeacherProfile> teacherRepository,
            IRepository<Group> groupRepository,
            IRepository<Department> departmentRepository)
        {
            _userRepository = userRepository;
            _studentRepository = studentRepository;
            _teacherRepository = teacherRepository;
            _groupRepository = groupRepository;
            _departmentRepository = departmentRepository;
        }

        // GET: Admin/Index
        public async Task<IActionResult> Index()
        {
            var users = await _userRepository.GetAllAsync();
            return View(users);
        }

        // GET: Admin/Users
        public async Task<IActionResult> Users()
        {
            var users = await _userRepository.GetAllAsync();
            return View(users);
        }

        // GET: Admin/CreateStudent
        public async Task<IActionResult> CreateStudent()
        {
            ViewBag.Groups = await _groupRepository.GetAllAsync();
            return View();
        }

        // POST: Admin/CreateStudent
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateStudent(string name, string email, int groupId)
        {
            try
            {
                // Використання Factory Method Pattern
                var user = UserFactory.CreateStudent(name, email);
                await _userRepository.AddAsync(user);

                // Створення профілю студента
                var studentProfile = new StudentProfile
                {
                    UserId = user.Id,
                    GroupId = groupId
                };
                await _studentRepository.AddAsync(studentProfile);

                TempData["Success"] = $"Студента {name} успішно створено! Пароль відправлено на email.";
                return RedirectToAction(nameof(Users));
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Помилка: {ex.Message}";
                ViewBag.Groups = await _groupRepository.GetAllAsync();
                return View();
            }
        }

        // GET: Admin/CreateTeacher
        public async Task<IActionResult> CreateTeacher()
        {
            ViewBag.Departments = await _departmentRepository.GetAllAsync();
            return View();
        }

        // POST: Admin/CreateTeacher
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateTeacher(string name, string email, int departmentId)
        {
            try
            {
                // Використання Factory Method Pattern
                var user = UserFactory.CreateTeacher(name, email);
                await _userRepository.AddAsync(user);

                // Створення профілю викладача
                var teacherProfile = new TeacherProfile
                {
                    UserId = user.Id,
                    DepartmentId = departmentId
                };
                await _teacherRepository.AddAsync(teacherProfile);

                TempData["Success"] = $"Викладача {name} успішно створено! Пароль відправлено на email.";
                return RedirectToAction(nameof(Users));
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Помилка: {ex.Message}";
                ViewBag.Departments = await _departmentRepository.GetAllAsync();
                return View();
            }
        }

        // GET: Admin/CreateAdmin
        public IActionResult CreateAdmin()
        {
            return View();
        }

        // POST: Admin/CreateAdmin
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateAdmin(string name, string email, string password, string confirmPassword)
        {
            try
            {
                if (password != confirmPassword)
                {
                    TempData["Error"] = "Паролі не співпадають!";
                    return View();
                }

                // Використання Factory Method Pattern
                var user = UserFactory.CreateAdmin(name, email, password);
                await _userRepository.AddAsync(user);

                TempData["Success"] = $"Адміністратора {name} успішно створено!";
                return RedirectToAction(nameof(Users));
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Помилка: {ex.Message}";
                return View();
            }
        }

        // GET: Admin/DeleteUser/5
        public async Task<IActionResult> DeleteUser(int id)
        {
            var user = await _userRepository.GetByIdAsync(id);
            if (user == null)
            {
                TempData["Error"] = "Користувача не знайдено!";
                return RedirectToAction(nameof(Users));
            }
            return View(user);
        }

        // POST: Admin/DeleteUser/5
        [HttpPost, ActionName("DeleteUser")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteUserConfirmed(int id)
        {
            try
            {
                await _userRepository.DeleteByIdAsync(id);
                TempData["Success"] = "Користувача успішно видалено!";
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Помилка: {ex.Message}";
            }
            return RedirectToAction(nameof(Users));
        }

        // GET: Admin/Statistics
        public async Task<IActionResult> Statistics()
        {
            var totalUsers = await _userRepository.CountAsync();
            var students = await _userRepository.FindAsync(u => u.Role == UserRole.Student);
            var teachers = await _userRepository.FindAsync(u => u.Role == UserRole.Teacher);
            var admins = await _userRepository.FindAsync(u => u.Role == UserRole.Admin);

            ViewBag.TotalUsers = totalUsers;
            ViewBag.Students = students.Count;
            ViewBag.Teachers = teachers.Count;
            ViewBag.Admins = admins.Count;

            return View();
        }
    }
}

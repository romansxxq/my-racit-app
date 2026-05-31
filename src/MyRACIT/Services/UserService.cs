using Microsoft.EntityFrameworkCore;
using MyRACIT.Data;
using MyRACIT.Models.Entities;
using MyRACIT.Services.Interfaces;
using System.Security.Cryptography;
using System.Text;

namespace MyRACIT.Services
{
    /// <summary>
    /// Сервіс для управління користувачами системи
    /// Реалізує Factory Method Pattern для створення користувачів різних ролей
    /// </summary>
    public class UserService : IUserService
    {
        private readonly MyRacitDbContext _context;
        
        public UserService(MyRacitDbContext context)
        {
            _context = context;
        }
        
        /// <summary>
        /// Створює студента (User + StudentProfile)
        /// </summary>
        public async Task<StudentProfile> CreateStudentAsync(string name, string email, string password, int groupId)
        {
            // Перевірка унікальності email
            if (await EmailExistsAsync(email))
            {
                throw new InvalidOperationException($"Користувач з email {email} вже існує!");
            }
            
            // Перевірка чи існує група
            var group = await _context.Groups.FindAsync(groupId);
            if (group == null)
            {
                throw new ArgumentException($"Група з ID {groupId} не знайдена!");
            }
            
            // Створюємо User
            var user = new User
            {
                Name = name,
                Email = email,
                PasswordHash = HashPassword(password),
                Role = UserRole.Student
            };
            
            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            
            // Створюємо StudentProfile
            var studentProfile = new StudentProfile
            {
                UserId = user.Id,
                GroupId = groupId
            };
            
            _context.StudentProfiles.Add(studentProfile);
            await _context.SaveChangesAsync();
            
            // Завантажуємо навігаційні властивості для повернення
            await _context.Entry(studentProfile)
                .Reference(s => s.User)
                .LoadAsync();
            await _context.Entry(studentProfile)
                .Reference(s => s.Group)
                .LoadAsync();
            
            return studentProfile;
        }
        
        /// <summary>
        /// Створює викладача (User + TeacherProfile)
        /// </summary>
        public async Task<TeacherProfile> CreateTeacherAsync(string name, string email, string password, int departmentId)
        {
            // Перевірка унікальності email
            if (await EmailExistsAsync(email))
            {
                throw new InvalidOperationException($"Користувач з email {email} вже існує!");
            }
            
            // Перевірка чи існує кафедра
            var department = await _context.Departments.FindAsync(departmentId);
            if (department == null)
            {
                throw new ArgumentException($"Кафедра з ID {departmentId} не знайдена!");
            }
            
            // Створюємо User
            var user = new User
            {
                Name = name,
                Email = email,
                PasswordHash = HashPassword(password),
                Role = UserRole.Teacher
            };
            
            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            
            // Створюємо TeacherProfile
            var teacherProfile = new TeacherProfile
            {
                UserId = user.Id,
                DepartmentId = departmentId
            };
            
            _context.TeacherProfiles.Add(teacherProfile);
            await _context.SaveChangesAsync();
            
            // Завантажуємо навігаційні властивості
            await _context.Entry(teacherProfile)
                .Reference(t => t.User)
                .LoadAsync();
            await _context.Entry(teacherProfile)
                .Reference(t => t.Department)
                .LoadAsync();
            
            return teacherProfile;
        }
        
        /// <summary>
        /// Створює адміністратора
        /// </summary>
        public async Task<User> CreateAdminAsync(string name, string email, string password)
        {
            // Перевірка унікальності email
            if (await EmailExistsAsync(email))
            {
                throw new InvalidOperationException($"Користувач з email {email} вже існує!");
            }
            
            var user = new User
            {
                Name = name,
                Email = email,
                PasswordHash = HashPassword(password),
                Role = UserRole.Admin
            };
            
            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            
            return user;
        }
        
        /// <summary>
        /// Перевіряє чи існує email в системі
        /// </summary>
        public async Task<bool> EmailExistsAsync(string email)
        {
            return await _context.Users
                .AnyAsync(u => u.Email.ToLower() == email.ToLower());
        }
        
        /// <summary>
        /// Отримує користувача за email
        /// </summary>
        public async Task<User?> GetUserByEmailAsync(string email)
        {
            return await _context.Users
                .FirstOrDefaultAsync(u => u.Email.ToLower() == email.ToLower());
        }
        
        /// <summary>
        /// Отримує користувача за ID
        /// </summary>
        public async Task<User?> GetUserByIdAsync(int id)
        {
            return await _context.Users.FindAsync(id);
        }
        
        /// <summary>
        /// Аутентифікація користувача
        /// </summary>
        public async Task<User?> AuthenticateAsync(string email, string password)
        {
            var user = await GetUserByEmailAsync(email);
            
            if (user == null)
                return null;
            
            // Перевіряємо пароль
            if (VerifyPassword(password, user.PasswordHash))
            {
                return user;
            }
            
            return null;
        }
        
        /// <summary>
        /// Змінює пароль користувача
        /// </summary>
        public async Task<bool> ChangePasswordAsync(int userId, string oldPassword, string newPassword)
        {
            var user = await GetUserByIdAsync(userId);
            
            if (user == null)
                return false;
            
            // Перевіряємо старий пароль
            if (!VerifyPassword(oldPassword, user.PasswordHash))
                return false;
            
            // Змінюємо на новий
            user.PasswordHash = HashPassword(newPassword);
            await _context.SaveChangesAsync();
            
            return true;
        }
        
        /// <summary>
        /// Оновлює профіль користувача
        /// </summary>
        public async Task<bool> UpdateUserProfileAsync(int userId, string name, string email)
        {
            var user = await GetUserByIdAsync(userId);
            
            if (user == null)
                return false;
            
            // Якщо змінюється email - перевіряємо унікальність
            if (user.Email.ToLower() != email.ToLower())
            {
                if (await EmailExistsAsync(email))
                {
                    throw new InvalidOperationException($"Email {email} вже зайнятий!");
                }
            }
            
            user.Name = name;
            user.Email = email;
            
            await _context.SaveChangesAsync();
            return true;
        }
        
        /// <summary>
        /// Отримує профіль студента
        /// </summary>
        public async Task<StudentProfile?> GetStudentProfileAsync(int userId)
        {
            return await _context.StudentProfiles
                .Include(s => s.User)
                .Include(s => s.Group)
                    .ThenInclude(g => g!.Specialty)
                .FirstOrDefaultAsync(s => s.UserId == userId);
        }
        
        /// <summary>
        /// Отримує профіль викладача
        /// </summary>
        public async Task<TeacherProfile?> GetTeacherProfileAsync(int userId)
        {
            return await _context.TeacherProfiles
                .Include(t => t.User)
                .Include(t => t.Department)
                .FirstOrDefaultAsync(t => t.UserId == userId);
        }
        
        /// <summary>
        /// Деактивує користувача (для майбутньої реалізації soft delete)
        /// </summary>
        public async Task<bool> DeactivateUserAsync(int userId)
        {
            var user = await GetUserByIdAsync(userId);
            
            if (user == null)
                return false;
            
            // TODO: Додати поле IsActive в User model
            // user.IsActive = false;
            // await _context.SaveChangesAsync();
            
            // Поки що просто повертаємо true
            return true;
        }
        
        // ===== PRIVATE METHODS =====
        
        /// <summary>
        /// Хешує пароль за допомогою SHA256
        /// TODO: Для production краще використати BCrypt або ASP.NET Identity
        /// </summary>
        private string HashPassword(string password)
        {
            using (var sha256 = SHA256.Create())
            {
                var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                return Convert.ToBase64String(hashedBytes);
            }
        }
        
        /// <summary>
        /// Перевіряє чи співпадає пароль з хешем
        /// </summary>
        private bool VerifyPassword(string password, string passwordHash)
        {
            var hashOfInput = HashPassword(password);
            return hashOfInput == passwordHash;
        }
    }
}

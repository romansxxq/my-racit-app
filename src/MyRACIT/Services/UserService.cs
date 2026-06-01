using Microsoft.EntityFrameworkCore;
using MyRACIT.Data;
using MyRACIT.Models.Entities;
using MyRACIT.Models.Exceptions;
using MyRACIT.Services.Interfaces;
using MyRACIT.Services.UserCreation;

namespace MyRACIT.Services
{
    /// <summary>
    /// Сервіс для управління користувачами системи.
    /// Делегує створення користувачів до класів UserCreation (Template Method Pattern).
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
            var creator = new StudentCreator(_context, groupId);
            var user = await creator.CreateAsync(name, email, password);

            return await _context.StudentProfiles
                .Include(s => s.User)
                .Include(s => s.Group)
                .FirstAsync(s => s.UserId == user.Id);
        }
        
        /// <summary>
        /// Створює викладача (User + TeacherProfile)
        /// </summary>
        public async Task<TeacherProfile> CreateTeacherAsync(string name, string email, string password, int departmentId)
        {
            var creator = new TeacherCreator(_context, departmentId);
            var user = await creator.CreateAsync(name, email, password);

            return await _context.TeacherProfiles
                .Include(t => t.User)
                .Include(t => t.Department)
                .FirstAsync(t => t.UserId == user.Id);
        }
        
        /// <summary>
        /// Створює адміністратора
        /// </summary>
        public async Task<User> CreateAdminAsync(string name, string email, string password)
        {
            var creator = new AdminCreator(_context);
            return await creator.CreateAsync(name, email, password);
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
            user.PasswordHash = UserCreatorBase.HashPassword(newPassword);
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
        /// Перевіряє чи співпадає пароль з хешем
        /// </summary>
        private static bool VerifyPassword(string password, string passwordHash)
        {
            return UserCreatorBase.HashPassword(password) == passwordHash;
        }
    }
}

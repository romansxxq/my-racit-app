using MyRACIT.Models.Entities;

namespace MyRACIT.Services.Interfaces
{
    /// <summary>
    /// Інтерфейс для управління користувачами системи
    /// </summary>
    public interface IUserService
    {
        /// <summary>
        /// Створює студента (User + StudentProfile)
        /// </summary>
        /// <returns>Створений StudentProfile</returns>
        Task<StudentProfile> CreateStudentAsync(string name, string email, string password, int groupId);
        
        /// <summary>
        /// Створює викладача (User + TeacherProfile)
        /// </summary>
        /// <returns>Створений TeacherProfile</returns>
        Task<TeacherProfile> CreateTeacherAsync(string name, string email, string password, int departmentId);
        
        /// <summary>
        /// Створює адміністратора (тільки User)
        /// </summary>
        /// <returns>Створений User</returns>
        Task<User> CreateAdminAsync(string name, string email, string password);
        
        /// <summary>
        /// Перевіряє чи існує користувач з таким email
        /// </summary>
        Task<bool> EmailExistsAsync(string email);
        
        /// <summary>
        /// Отримує користувача за email
        /// </summary>
        Task<User?> GetUserByEmailAsync(string email);
        
        /// <summary>
        /// Отримує користувача за ID
        /// </summary>
        Task<User?> GetUserByIdAsync(int id);
        
        /// <summary>
        /// Аутентифікує користувача (перевіряє email та пароль)
        /// </summary>
        /// <returns>User якщо успішно, null якщо невірні дані</returns>
        Task<User?> AuthenticateAsync(string email, string password);
        
        /// <summary>
        /// Змінює пароль користувача
        /// </summary>
        Task<bool> ChangePasswordAsync(int userId, string oldPassword, string newPassword);
        
        /// <summary>
        /// Оновлює профіль користувача (ім'я, email)
        /// </summary>
        Task<bool> UpdateUserProfileAsync(int userId, string name, string email);
        
        /// <summary>
        /// Отримує StudentProfile за userId
        /// </summary>
        Task<StudentProfile?> GetStudentProfileAsync(int userId);
        
        /// <summary>
        /// Отримує TeacherProfile за userId
        /// </summary>
        Task<TeacherProfile?> GetTeacherProfileAsync(int userId);
        
        /// <summary>
        /// Видаляє користувача (soft delete - деактивація)
        /// </summary>
        Task<bool> DeactivateUserAsync(int userId);
    }
}

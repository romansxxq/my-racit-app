using MyRACIT.Models.Entities;
using System.Security.Cryptography;
using System.Text;

namespace MyRACIT.Factories
{
    /// <summary>
    /// Фабрика для створення користувачів різних ролей
    /// Реалізує Factory Method Pattern
    /// </summary>
    public static class UserFactory
    {
        /// <summary>
        /// Створити студента з автоматично згенерованим паролем
        /// </summary>
        public static User CreateStudent(string name, string email)
        {
            var password = GeneratePassword();
            var user = new User
            {
                Name = name,
                Email = email,
                Role = UserRole.Student,
                PasswordHash = HashPassword(password)
            };
            
            // TODO: Відправити email з паролем
            Console.WriteLine($"[INFO] Створено студента: {name}, пароль: {password}");
            
            return user;
        }

        /// <summary>
        /// Створити викладача з автоматично згенерованим паролем
        /// </summary>
        public static User CreateTeacher(string name, string email)
        {
            var password = GeneratePassword();
            var user = new User
            {
                Name = name,
                Email = email,
                Role = UserRole.Teacher,
                PasswordHash = HashPassword(password)
            };
            
            // TODO: Відправити email з паролем
            Console.WriteLine($"[INFO] Створено викладача: {name}, пароль: {password}");
            
            return user;
        }

        /// <summary>
        /// Створити адміністратора з вказаним паролем
        /// </summary>
        public static User CreateAdmin(string name, string email, string password)
        {
            if (string.IsNullOrWhiteSpace(password) || password.Length < 8)
            {
                throw new ArgumentException("Пароль адміністратора має містити мінімум 8 символів", nameof(password));
            }

            var user = new User
            {
                Name = name,
                Email = email,
                Role = UserRole.Admin,
                PasswordHash = HashPassword(password)
            };
            
            Console.WriteLine($"[INFO] Створено адміністратора: {name}");
            
            return user;
        }

        /// <summary>
        /// Створити користувача з вказаною роллю та паролем
        /// </summary>
        public static User CreateUser(string name, string email, UserRole role, string? password = null)
        {
            return role switch
            {
                UserRole.Student => CreateStudent(name, email),
                UserRole.Teacher => CreateTeacher(name, email),
                UserRole.Admin => CreateAdmin(name, email, password ?? GeneratePassword()),
                _ => throw new ArgumentException($"Невідома роль: {role}", nameof(role))
            };
        }

        /// <summary>
        /// Згенерувати випадковий пароль
        /// </summary>
        private static string GeneratePassword()
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789!@#$%";
            var random = new Random();
            var password = new StringBuilder();

            // Генеруємо пароль з 12 символів
            for (int i = 0; i < 12; i++)
            {
                password.Append(chars[random.Next(chars.Length)]);
            }

            return password.ToString();
        }

        /// <summary>
        /// Хешувати пароль за допомогою SHA256
        /// TODO: В продакшені використовувати BCrypt або Argon2
        /// </summary>
        private static string HashPassword(string password)
        {
            using var sha256 = SHA256.Create();
            var bytes = Encoding.UTF8.GetBytes(password);
            var hash = sha256.ComputeHash(bytes);
            return Convert.ToBase64String(hash);
        }

        /// <summary>
        /// Перевірити пароль користувача
        /// </summary>
        public static bool VerifyPassword(string password, string passwordHash)
        {
            var hash = HashPassword(password);
            return hash == passwordHash;
        }
    }
}

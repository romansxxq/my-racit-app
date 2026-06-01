using Microsoft.EntityFrameworkCore;
using MyRACIT.Data;
using MyRACIT.Models.Entities;
using MyRACIT.Models.Exceptions;
using System.Security.Cryptography;
using System.Text;

namespace MyRACIT.Services.UserCreation
{
    /// <summary>
    /// Template Method Pattern — базовий клас для створення користувачів.
    /// Фіксує незмінний скелет алгоритму, підкласи перевизначають змінні кроки.
    /// </summary>
    public abstract class UserCreatorBase
    {
        protected readonly MyRacitDbContext _context;

        protected UserCreatorBase(MyRacitDbContext context)
        {
            _context = context;
        }

        // =============================================
        // TEMPLATE METHOD — скелет алгоритму створення
        // =============================================
        public async Task<User> CreateAsync(string name, string email, string password)
        {
            // Крок 1: спільний — перевірка унікальності email
            await ValidateEmailAsync(email);

            // Крок 2: різний — підклас перевіряє свої залежності (група / кафедра / нічого)
            await ValidateDependenciesAsync();

            // Крок 3: спільний — створення User з відповідною роллю
            var user = new User
            {
                Name = name,
                Email = email,
                PasswordHash = HashPassword(password),
                Role = GetRole()
            };
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            // Крок 4: різний — підклас створює профіль (або нічого для Admin)
            await CreateProfileAsync(user);

            return user;
        }


        /// <summary>Повертає роль користувача</summary>
        protected abstract UserRole GetRole();

        /// <summary>Перевіряє залежні сутності (група, кафедра тощо)</summary>
        protected abstract Task ValidateDependenciesAsync();

        /// <summary>Створює профіль відповідного типу після збереження User</summary>
        protected abstract Task CreateProfileAsync(User user);

        // =============================================
        // СПІЛЬНІ КРОКИ — реалізовані в базовому класі
        // =============================================

        private async Task ValidateEmailAsync(string email)
        {
            if (await _context.Users.AnyAsync(u => u.Email.ToLower() == email.ToLower()))
                throw new UserAlreadyExistsException(email);
        }

        internal static string HashPassword(string password)
        {
            using var sha256 = SHA256.Create();
            var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
            return Convert.ToBase64String(hashedBytes);
        }
    }
}

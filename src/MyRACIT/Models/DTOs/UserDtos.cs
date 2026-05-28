using System.ComponentModel.DataAnnotations;

namespace MyRACIT.Models.DTOs
{
    /// <summary>
    /// DTO для створення студента
    /// </summary>
    public class CreateStudentDto
    {
        [Required(ErrorMessage = "ПІБ є обов'язковим")]
        [MaxLength(200)]
        public string Name { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "Email є обов'язковим")]
        [EmailAddress(ErrorMessage = "Невірний формат email")]
        public string Email { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "Пароль є обов'язковим")]
        [MinLength(6, ErrorMessage = "Пароль має бути мінімум 6 символів")]
        public string Password { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "Виберіть групу")]
        public int GroupId { get; set; }
    }
    
    /// <summary>
    /// DTO для створення викладача
    /// </summary>
    public class CreateTeacherDto
    {
        [Required(ErrorMessage = "ПІБ є обов'язковим")]
        [MaxLength(200)]
        public string Name { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "Email є обов'язковим")]
        [EmailAddress(ErrorMessage = "Невірний формат email")]
        public string Email { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "Пароль є обов'язковим")]
        [MinLength(6, ErrorMessage = "Пароль має бути мінімум 6 символів")]
        public string Password { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "Виберіть кафедру")]
        public int DepartmentId { get; set; }
    }
    
    /// <summary>
    /// DTO для авторизації
    /// </summary>
    public class LoginDto
    {
        [Required(ErrorMessage = "Email є обов'язковим")]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "Пароль є обов'язковим")]
        public string Password { get; set; } = string.Empty;
    }
    
    /// <summary>
    /// DTO для зміни пароля
    /// </summary>
    public class ChangePasswordDto
    {
        [Required(ErrorMessage = "Введіть старий пароль")]
        public string OldPassword { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "Введіть новий пароль")]
        [MinLength(6, ErrorMessage = "Новий пароль має бути мінімум 6 символів")]
        public string NewPassword { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "Підтвердіть новий пароль")]
        [Compare("NewPassword", ErrorMessage = "Паролі не співпадають")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }
    
    /// <summary>
    /// DTO для оновлення профілю
    /// </summary>
    public class UpdateProfileDto
    {
        [Required(ErrorMessage = "ПІБ є обов'язковим")]
        [MaxLength(200)]
        public string Name { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "Email є обов'язковим")]
        [EmailAddress(ErrorMessage = "Невірний формат email")]
        public string Email { get; set; } = string.Empty;
    }
}

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace MyRACIT.Models.Entities
{
    public class User
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "ПІБ є обов'язковим!")]
        [MaxLength(200)]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email є обов'язковим!")]
        [EmailAddress(ErrorMessage = "Невірний формат email!")]
        [MaxLength(200)]
        public string Email { get; set; } = string.Empty;

        [Required]
        public UserRole Role { get; set; }

        public override string ToString()
        {
            return $"{Name} ({Email}) - {Role}";
        }
    }
}
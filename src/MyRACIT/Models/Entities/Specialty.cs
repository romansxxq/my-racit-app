using System.ComponentModel.DataAnnotations;

namespace MyRACIT.Models.Entities
{
    public class Specialty
    {
        [Key]
        public int Id { get; set; }
        [Required(ErrorMessage = "Код спеціальності є обов'язковим!")]
        [MaxLength(10)]
        public string Code { get; set; } = string.Empty;

        [Required(ErrorMessage = "Назва спеціальності є обов'язковою!")]
        [MaxLength(200)]
        public string Name { get; set; } = string.Empty;

        public override string ToString() => $"{Code} - {Name}";
    }
}
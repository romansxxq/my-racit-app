using System.ComponentModel.DataAnnotations;
namespace MyRACIT.Models.Entities
{
    public class Department
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Назва кафедри є обов'язковою!")]
        [MaxLength(200)]
        public string Name { get; set; } = string.Empty;

        public override string ToString() => Name;
    }
}
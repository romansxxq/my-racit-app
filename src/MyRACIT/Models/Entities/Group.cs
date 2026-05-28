using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace MyRACIT.Models.Entities
{
    public class Group
    {
        [Key]
        public int Id { get; set; }
        [Required(ErrorMessage = "Назва групи є обов'язковою!")]
        [MaxLength(20)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [Range(1, 4, ErrorMessage = "Рік навчання повинен бути від 1 до 4!")]
        public int StudyYear { get; set; }

        [Required]
        public int SpecialtyId { get; set; }

        [ForeignKey("SpecialtyId")]
        public Specialty? Specialty { get; set; }
        public override string ToString() => Name;
    }
}
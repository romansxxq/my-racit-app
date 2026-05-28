using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace MyRACIT.Models.Entities
{
    public class Grade
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public int StudentId { get; set; }
        [ForeignKey("StudentId")]
        public StudentProfile? Student { get; set; }
        [Required]
        public int AssignmentId { get; set; }
        [ForeignKey("AssignmentId")]
        public Assignment? Assignment { get; set; }
        [Required(ErrorMessage = "Оцінка є обов'язковою!")]
        public int Value { get; set; }
        [Required]
        public DateTime DateIssued { get; set; } = DateTime.UtcNow;
        [DataType(DataType.Html)]
        public string? Feedback { get; set; }

        public override string ToString() => $"{Value} балів -> {Student?.User?.Name}";
    }
}
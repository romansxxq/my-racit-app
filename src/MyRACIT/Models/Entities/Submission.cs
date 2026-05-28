using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace MyRACIT.Models.Entities
{
    public class Submission
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public int AssignmentId { get; set; }
        [ForeignKey("AssignmentId")]
        public Assignment? Assignment { get; set; }
        [Required]
        public int StudentId { get; set; }
        [ForeignKey("StudentId")]
        public StudentProfile? Student { get; set; }
        [Required]
        public string FilePath { get; set; } = string.Empty;
        [Required]
        public DateTime SubmittedAt { get; set; } = DateTime.UtcNow;
        public override string ToString() => $"Лаба від {Student?.User?.Name} ({Assignment?.Title})";
    }
}
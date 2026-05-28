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
        
        // Navigation property to Grade (one-to-one)
        [InverseProperty("Submission")]
        public Grade? Grade { get; set; }
        
        // Aliases для Controllers (для зворотної сумісності)
        [NotMapped]
        public int StudentProfileId
        {
            get => StudentId;
            set => StudentId = value;
        }
        
        [NotMapped]
        public StudentProfile? StudentProfile => Student;
        
        [NotMapped]
        public string Content { get; set; } = string.Empty;
        
        public override string ToString() => $"Лаба від {Student?.User?.Name} ({Assignment?.Title})";
    }
}
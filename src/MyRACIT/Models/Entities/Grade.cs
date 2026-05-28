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
        public int SubmissionId { get; set; }
        [ForeignKey("SubmissionId")]
        public Submission? Submission { get; set; }
        
        public int? CourseId { get; set; }
        [ForeignKey("CourseId")]
        public Course? Course { get; set; }
        
        [Required(ErrorMessage = "Оцінка є обов'язковою!")]
        public int Value { get; set; }
        [Required]
        public DateTime DateIssued { get; set; } = DateTime.UtcNow;
        [DataType(DataType.Html)]
        public string? Feedback { get; set; }
        
        // Aliases для Controllers (для зворотної сумісності)
        [NotMapped]
        public int StudentProfileId
        {
            get => StudentId;
            set => StudentId = value;
        }
        
        [NotMapped]
        public int Points
        {
            get => Value;
            set => Value = value;
        }
        
        [NotMapped]
        public DateTime GradedAt
        {
            get => DateIssued;
            set => DateIssued = value;
        }
        
        [NotMapped]
        public string? Comment
        {
            get => Feedback;
            set => Feedback = value;
        }


        public override string ToString() => $"{Value} балів -> {Student?.User?.Name}";
    }
}
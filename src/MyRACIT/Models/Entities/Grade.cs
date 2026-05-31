using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace MyRACIT.Models.Entities
{
    public class Grade
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int SubmissionId { get; set; }
        [ForeignKey("SubmissionId")]
        public Submission? Submission { get; set; }
        
        [Required(ErrorMessage = "Оцінка є обов'язковою!")]
        [Range(0, 5, ErrorMessage = "Оцінка повинна бути від 0 до 5!")]
        public int Value { get; set; }
        
        [Required]
        public DateTime DateIssued { get; set; } = DateTime.UtcNow;
        
        [DataType(DataType.Html)]
        [MaxLength(2000)]
        public string? Feedback { get; set; }
        
        public int GradedBy { get; set; }
        [ForeignKey("GradedBy")]
        public TeacherProfile? GradedByTeacher { get; set; }
        
        [NotMapped]
        public int StudentProfileId => Submission?.StudentId ?? 0;
        
        [NotMapped]
        public StudentProfile? Student => Submission?.Student;
        
        [NotMapped]
        public int? CourseId => Submission?.Assignment?.CourseId;
        
        [NotMapped]
        public Course? Course => Submission?.Assignment?.Course;
        
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
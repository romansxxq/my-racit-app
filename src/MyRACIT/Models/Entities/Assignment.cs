using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace MyRACIT.Models.Entities
{
    public class Assignment
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Назва завдання є обов'язковою")]
        [MaxLength(200)]
        public string Title { get; set; } = string.Empty;
        
        [DataType(DataType.Html)]
        [MaxLength(2000)]
        public string? Description { get; set; }
        
        /// <summary>
        /// ✅ ДОДАНО: Тип завдання (домашка, лаба, проєкт, тест, іспит)
        /// </summary>
        [Required]
        public AssignmentType Type { get; set; } = AssignmentType.Homework;
        
        [Required(ErrorMessage = "Дедлайн є обов'язковим")]
        [DataType(DataType.DateTime)]
        public DateTime Deadline { get; set; }
        
        [Required]
        [Range(0, 5, ErrorMessage = "Макс. бал повинен бути від 0 до 5!")]
        public int MaxGrade { get; set; } = 5;
        
        [Required]
        public int CourseId { get; set; }
        [ForeignKey("CourseId")]
        public Course? Course { get; set; }
        
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        
        // ✅ ДОДАНО: Колекції файлів та посилань
        public ICollection<AssignmentFile> Files { get; set; } = new List<AssignmentFile>();
        public ICollection<AssignmentLink> Links { get; set; } = new List<AssignmentLink>();
        public ICollection<Submission> Submissions { get; set; } = new List<Submission>();
        
        // Aliases для Controllers (для зворотної сумісності)
        [NotMapped]
        public DateTime DueDate
        {
            get => Deadline;
            set => Deadline = value;
        }
        
        [NotMapped]
        public int MaxPoints
        {
            get => MaxGrade;
            set => MaxGrade = value;
        }
        
        [NotMapped]
        public int MaxScore
        {
            get => MaxGrade;
            set => MaxGrade = value;
        }

        public override string ToString() => $"{Title} (Група: {Course?.Group?.Name})";
    }
    
    /// <summary>
    /// ✅ ДОДАНО: Типи завдань
    /// </summary>
    public enum AssignmentType
    {
        Homework,  // Домашнє завдання
        Lab,       // Лабораторна робота
        Project,   // Проєкт
        Quiz,      // Тест
        Exam       // Іспит
    }
}
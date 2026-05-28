using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace MyRACIT.Models.Entities
{
    public class Assignment
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Назва завдання є обов'язковою")]
        public string Title { get; set; } = string.Empty;
        [DataType(DataType.Html)]
        public string? Description { get; set; }
        [Required(ErrorMessage = "Дедлайн є обов'язковим")]
        [DataType(DataType.DateTime)]
        public DateTime Deadline { get; set; }
        [Required]
        [Range(0, 5, ErrorMessage = "Макс. бал повинен быть от 0 до 5!")]
        public int MaxGrade { get; set; }
        [Required]
        public int CourseId { get; set; }
        [ForeignKey("CourseId")]
        public Course? Course { get; set; }

        public override string ToString() => $"{Title} (Група: {Course?.Group?.Name})";
    }
}
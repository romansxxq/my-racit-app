using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace MyRACIT.Models.Entities
{
    public class Course
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int SubjectId { get; set; }
        [ForeignKey("SubjectId")]
        public Subject? Subject { get; set; }
        [Required]
        public int TeacherId { get; set; }
        [ForeignKey("TeacherId")]
        public TeacherProfile? Teacher { get; set; }
        [Required]
        public int GroupId { get; set; }
        [ForeignKey("GroupId")]
        public Group? Group { get; set; }
        [Required]
        [DataType(DataType.Date)]
        public DateTime StartDate { get; set; } //початок семестру
        [Required]
        [DataType(DataType.Date)]
        public DateTime EndDate { get; set; } //кінець семестру

        public override string ToString() => $"{Subject?.Title} - {Teacher?.User?.Name} ({Group?.Name})";
    }
}
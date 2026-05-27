using System.ComponentModel.DataAnnotations;

namespace MyRACIT.Models.Entities
{
    public class Subject
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Назва предмету/дисципліни є обов'язковою!")]
        [StringLength(100)]
        public string Title { get; set; } = string.Empty;

        public override string ToString() => Title;
    }
}
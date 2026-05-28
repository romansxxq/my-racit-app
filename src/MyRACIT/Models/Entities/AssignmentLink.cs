using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace MyRACIT.Models.Entities
{
    public class AssignmentLink
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int AssignmentId { get; set; }
        [ForeignKey("AssignmentId")]
        public Assignment? Assignment { get; set; }

        [Required]
        [MaxLength(1000)]
        [DataType(DataType.Url)]
        public string Url { get; set; } = string.Empty;

        [MaxLength(200)]
        public string? Label { get; set; }

        public override string ToString() => Label ?? Url;
    }
}

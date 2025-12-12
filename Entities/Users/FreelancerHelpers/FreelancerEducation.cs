using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Entities.Users.FreelancerHelpers
{
    /// <summary>
    /// Represents an educational background entry for the freelancer.
    /// </summary>
    [Table("freelancer_education")]
    public class FreelancerEducation
    {
        [Key]
        public int Id { get; set; }

        [ForeignKey("Freelancer")]
        public string FreelancerId { get; set; }
        public virtual Freelancer Freelancer { get; set; }

        [Required]
        [MaxLength(100)]
        public string School { get; set; }

        public DateTime DateStart { get; set; }

        public DateTime? DateEnd { get; set; }

        [MaxLength(100)]
        public string Degree { get; set; }

        [MaxLength(100)]
        public string FieldOfStudy { get; set; }
    }
}
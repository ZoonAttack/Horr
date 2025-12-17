using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Entities.Users.FreelancerHelpers
{
    /// <summary>
    /// Represents a language spoken by the freelancer.
    /// </summary>
    [Table("freelancer_languages")]
    public class FreelancerLanguage
    {
        [Key]
        public int Id { get; set; }

        [ForeignKey("Freelancer")]
        public string FreelancerId { get; set; }
        public virtual Freelancer Freelancer { get; set; }

        [Required]
        [MaxLength(50)]
        public string Name { get; set; } // e.g., English, Spanish

        [Required]
        [MaxLength(50)]
        public string Level { get; set; } // e.g., Native, Fluent, Conversational
    }
}
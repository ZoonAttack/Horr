using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Entities.Users.FreelancerHelpers
{
    /// <summary>
    /// Represents a project or experience detail entry for the freelancer.
    /// </summary>
    [Table("freelancer_experience_details")]
    public class FreelancerExperienceDetail
    {
        [Key]
        public int Id { get; set; }

        [ForeignKey("Freelancer")]
        public string FreelancerId { get; set; }
        public virtual Freelancer Freelancer { get; set; }

        [Required]
        [MaxLength(255)]
        public string Subject { get; set; }

        [Column(TypeName = "text")]
        public string Description { get; set; }
    }
}
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Entities.Users.FreelancerHelpers
{

    /// <summary>
    /// Represents an employment history entry for the freelancer.
    /// </summary>
    [Table("freelancer_employment_history")]
    public class FreelancerEmployment
    {
        [Key]
        public int Id { get; set; }

        [ForeignKey("Freelancer")]
        public string FreelancerId { get; set; }
        public virtual Freelancer Freelancer { get; set; }

        [Required]
        [MaxLength(100)]
        public string Company { get; set; }

        [Required]
        [MaxLength(50)]
        public string City { get; set; }

        [Required]
        [MaxLength(50)]
        public string Country { get; set; }

        [Required]
        [MaxLength(100)]
        public string Title { get; set; }

        public bool CurrentlyWorkThere { get; set; }

        public DateTime FromDate { get; set; }

        public DateTime? ToDate { get; set; }
    }
}
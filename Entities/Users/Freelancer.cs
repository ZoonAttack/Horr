// --- Updated Freelancer Entity in Entities.Users ---

using Entities.Communication;
using Entities.Marketplace;
using Entities.Project;
using Entities.Skill;
using Entities.Users.FreelancerHelpers;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Entities.Users
{
    /// <summary>
    /// Profile information for users with the 'freelancer' role.
    /// </summary>
    [Table("freelancers")]
    public class Freelancer
    {
        [Key]
        [ForeignKey("User")]
        public string UserId { get; set; }
        public virtual User User { get; set; }

        [Column(TypeName = "text")]
        public string Bio { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal? HourlyRate { get; set; }

        [MaxLength(50)]
        public string Availability { get; set; }

        public int? YearsOfExperience { get; set; }

        [MaxLength(255)]
        [Url]
        public string PortfolioUrl { get; set; }


        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public DateTime CreatedAt { get; set; }

        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public DateTime UpdatedAt { get; set; }

        // --- Navigation Properties ---
        public virtual ICollection<FreelancerSkill> FreelancerSkills { get; set; } = new List<FreelancerSkill>();
        public virtual ICollection<Proposal> Proposals { get; set; } = new List<Proposal>();
        public virtual ICollection<Service> Services { get; set; } = new List<Service>();
        public virtual ICollection<Order> Orders { get; set; } = new List<Order>();
        public virtual ICollection<Chat> Chats { get; set; } = new List<Chat>();
        public virtual ICollection<Entities.Payment.Payment> Payments { get; set; } = new List<Entities.Payment.Payment>();
        public virtual ICollection<Contract> Contracts { get; set; } = new List<Contract>();

        public virtual ICollection<FreelancerLanguage> Languages { get; set; } = new List<FreelancerLanguage>();
        public virtual ICollection<FreelancerEducation> Education { get; set; } = new List<FreelancerEducation>();
        public virtual ICollection<FreelancerExperienceDetail> ExperienceDetails { get; set; } = new List<FreelancerExperienceDetail>();
        public virtual ICollection<FreelancerEmployment> EmploymentHistory { get; set; } = new List<FreelancerEmployment>();
        
    }
}
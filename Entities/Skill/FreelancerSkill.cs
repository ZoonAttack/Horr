using Entities.Enums;
using Entities.Users;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Entities.Skill
{
    /// <summary>
    /// Junction table linking Freelancers to Skills with a proficiency level.
    /// </summary>
    [Table("freelancer_skills")]
    [Index(nameof(FreelancerId))]
    [Index(nameof(SkillId))]
    public class FreelancerSkill
    {
        [Key, Column(Order = 0)]
        [ForeignKey("Freelancer")]
        public string FreelancerId { get; set; }

        [Key, Column(Order = 1)]
        [ForeignKey("Skill")]
        public string SkillId { get; set; }

        public ProficiencyLevel ProficiencyLevel { get; set; }

        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public DateTime CreatedAt { get; set; }

        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public DateTime UpdatedAt { get; set; }

        // --- Navigation Properties ---
        public virtual Freelancer Freelancer { get; set; }
        public virtual Skill Skill { get; set; }
    }
}

using Entities.Common;
using Entities.Enums;
using Entities.Skill;
using Entities.Users;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Entities.Project
{
    [Table("job_posts")]
    public class JobPost : ISoftDeletable
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(200)]
        public string Title { get; set; } = string.Empty;

        [Required]
        public string Description { get; set; } = string.Empty;

        [Column(TypeName = "decimal(18,2)")]
        public decimal BudgetMin { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal BudgetMax { get; set; }

        public JobType JobType { get; set; }

        public DateTime PostedAt { get; set; } = DateTime.UtcNow;

        [Required]
        public string ClientId { get; set; } = string.Empty;

        [ForeignKey(nameof(ClientId))]
        public virtual User Client { get; set; } = null!;

        public bool IsDeleted { get; set; }

        public virtual ICollection<JobSkill> JobSkills { get; set; } = new List<JobSkill>();
        public virtual ICollection<SavedJob> SavedByFreelancers { get; set; } = new List<SavedJob>();
    }
}

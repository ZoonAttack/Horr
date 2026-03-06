using Entities.Users;
using System.ComponentModel.DataAnnotations.Schema;

namespace Entities.Project
{
    [Table("saved_jobs")]
    public class SavedJob
    {
        public string FreelancerId { get; set; } = string.Empty;
        
        [ForeignKey(nameof(FreelancerId))]
        public virtual User Freelancer { get; set; } = null!;

        public int JobPostId { get; set; }

        [ForeignKey(nameof(JobPostId))]
        public virtual JobPost JobPost { get; set; } = null!;

        public DateTime SavedAt { get; set; } = DateTime.UtcNow;
    }
}

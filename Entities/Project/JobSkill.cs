using Entities.Project;
using System.ComponentModel.DataAnnotations.Schema;

namespace Entities.Skill
{
    [Table("job_skills")]
    public class JobSkill
    {
        public int JobPostId { get; set; }

        [ForeignKey(nameof(JobPostId))]
        public virtual JobPost JobPost { get; set; } = null!;

        public string SkillId { get; set; } = string.Empty;

        [ForeignKey(nameof(SkillId))]
        public virtual Skill Skill { get; set; } = null!;
    }
}

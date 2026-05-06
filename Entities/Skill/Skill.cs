using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Entities.Project;

namespace Entities.Skill
{
    /// <summary>
    /// Represents a reusable skill (e.g., "C#", "Graphic Design").
    /// </summary>
    [Table("skills")]
    [Index(nameof(Name), IsUnique = true)]
    [Index(nameof(Category))]
    public class Skill
    {
        [Key]
        public string Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Name { get; set; }

        [Required]
        [ForeignKey(nameof(Category))]
        public string CategoryId { get; set; } = string.Empty;

        public virtual Entities.Project.Category Category { get; set; } = null!;

        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        // --- Navigation Properties ---
        public virtual ICollection<FreelancerSkill> FreelancerSkills { get; set; } = new List<FreelancerSkill>();
    }
}

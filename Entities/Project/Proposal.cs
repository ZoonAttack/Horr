using Entities.Enums;
using Entities.User;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Entities.Project
{
    /// <summary>
    /// A freelancer's bid or application for a ClientProject.
    /// </summary>
    [Table("proposals")]
    [Index(nameof(ProjectId))]
    [Index(nameof(FreelancerId))]
    [Index(nameof(Status))]
    public class Proposal
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }

        [Required]
        [ForeignKey("Project")]
        public long ProjectId { get; set; }

        [Required]
        [ForeignKey("Freelancer")]
        public long FreelancerId { get; set; }

        [Column(TypeName = "text")]
        public string CoverLetter { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal? ProposedAmount { get; set; }

        [MaxLength(50)]
        public string EstimatedDuration { get; set; }

        public ProposalStatus Status { get; set; } = ProposalStatus.Pending;

        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public DateTime CreatedAt { get; set; }

        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public DateTime UpdatedAt { get; set; }

        // --- Navigation Properties ---
        [InverseProperty("Proposals")]
        public virtual ClientProject Project { get; set; }
        public virtual Freelancer Freelancer { get; set; }

        public virtual ICollection<Delivery> Deliveries { get; set; } = new List<Delivery>();
    }
}

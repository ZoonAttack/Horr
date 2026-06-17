using Entities.Common;
using Entities.Enums;
using Entities.Review;
using Entities.Users;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Entities.Project
{
    /// <summary>
    /// Represents a formal contract created when a Proposal is accepted.
    /// One-to-one with Proposal. Supports soft-delete via ISoftDeletable.
    /// </summary>
    [Table("contracts")]
    [Index(nameof(ProposalId), IsUnique = true)]
    [Index(nameof(ClientId))]
    [Index(nameof(FreelancerId))]
    [Index(nameof(Status))]
    [Index(nameof(IsDeleted))]
    public class Contract : ISoftDeletable
    {
        [Key]
        public int Id { get; set; }

        // ── One-to-one: Contract → Proposal ──────────────────────────────
        public int? ProposalId { get; set; }

        [ForeignKey(nameof(ProposalId))]
        public virtual Proposal? Proposal { get; set; }

        // ── FK: JobPost ──────────────────────────────────────────────────
        public string? JobPostId { get; set; }

        [ForeignKey(nameof(JobPostId))]
        public virtual JobPost? JobPost { get; set; }

        // ── FK: Client (User) ─────────────────────────────────────────────
        [Required]
        public string ClientId { get; set; } = string.Empty;

        [ForeignKey(nameof(ClientId))]
        public virtual User Client { get; set; } = null!;

        // ── FK: Freelancer (User) ─────────────────────────────────────────
        [Required]
        public string FreelancerId { get; set; } = string.Empty;

        [ForeignKey(nameof(FreelancerId))]
        public virtual User Freelancer { get; set; } = null!;

        // ── Financial ────────────────────────────────────────────────────
        [Column(TypeName = "decimal(18,2)")]
        public decimal AgreedRate { get; set; }

        public string? CustomJobDescription { get; set; }

        // ── State ─────────────────────────────────────────────────────────
        public ContractStatus Status { get; set; } = ContractStatus.Active;

        public DateTime StartedAt { get; set; } = DateTime.UtcNow;

        public DateTime? AcceptedAt { get; set; }

        public DateTime? RejectedAt { get; set; }

        public DateTime? ClosedAt { get; set; }

        // ── Timestamps ────────────────────────────────────────────────────
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // ── Soft Delete (ISoftDeletable) ──────────────────────────────────
        public bool IsDeleted { get; set; } = false;

        // ── Navigation ────────────────────────────────────────────────────
        public virtual ICollection<WorkDelivery> WorkDeliveries { get; set; } = new List<WorkDelivery>();
        public virtual ICollection<ContractDelivery> ContractDeliveries { get; set; } = new List<ContractDelivery>();
        public virtual ICollection<ContractReview> ContractReviews { get; set; } = new List<ContractReview>();
        public virtual ICollection<ContractMilestone> ContractMilestones { get; set; } = new List<ContractMilestone>();
    }
}

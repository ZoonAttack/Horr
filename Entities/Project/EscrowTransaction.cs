using Entities.Common;
using Entities.Enums;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Entities.Project
{
    /// <summary>
    /// Tracks every money movement in and out of the escrow system.
    /// </summary>
    [Table("escrow_transactions")]
    public class EscrowTransaction : ISoftDeletable
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public int ContractId { get; set; }

        [ForeignKey(nameof(ContractId))]
        public virtual Contract Contract { get; set; } = null!;

        public Guid? ContractMilestoneId { get; set; }

        [ForeignKey(nameof(ContractMilestoneId))]
        public virtual ContractMilestone? ContractMilestone { get; set; }

        [Required]
        public EscrowTransactionType Type { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Amount { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal PlatformFeeFromClient { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal PlatformFeeFromFreelancer { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal NetToFreelancer { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Required]
        public EscrowStatus Status { get; set; } = EscrowStatus.Held;

        [Column(TypeName = "decimal(18,2)")]
        public decimal? ClientPercentage { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal? FreelancerPercentage { get; set; }

        public bool IsDeleted { get; set; } = false;
    }
}

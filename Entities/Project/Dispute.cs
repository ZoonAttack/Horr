using Entities.Common;
using Entities.Enums;
using Entities.Users;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Entities.Project
{
    /// <summary>
    /// Represents an escalated unresolvable dispute over contract deliveries, resolved by system administrators.
    /// </summary>
    [Table("disputes")]
    public class Dispute : ISoftDeletable
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public int ContractId { get; set; }

        [ForeignKey(nameof(ContractId))]
        public virtual Contract Contract { get; set; } = null!;

        [Required]
        public Guid ContractDeliveryId { get; set; }

        [ForeignKey(nameof(ContractDeliveryId))]
        public virtual ContractDelivery ContractDelivery { get; set; } = null!;

        [Required]
        public string OpenedByUserId { get; set; } = string.Empty;

        [ForeignKey(nameof(OpenedByUserId))]
        public virtual User OpenedByUser { get; set; } = null!;

        [Required]
        [Column(TypeName = "text")]
        public string Reason { get; set; } = string.Empty;

        public DateTime OpenedAt { get; set; } = DateTime.UtcNow;

        [Required]
        public DisputeStatus Status { get; set; } = DisputeStatus.Open;

        public string? AdminId { get; set; }

        [ForeignKey(nameof(AdminId))]
        public virtual User? Admin { get; set; }

        [Column(TypeName = "text")]
        public string? AdminDecision { get; set; }

        public DateTime? ResolvedAt { get; set; }

        public bool IsDeleted { get; set; } = false;
    }
}

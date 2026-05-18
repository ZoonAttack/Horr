using Entities.Common;
using Entities.Enums;
using Entities.Users;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Entities.Project
{
    /// <summary>
    /// Represents a client's request to revision/redo a submitted work delivery.
    /// Can be escalated and resolved by a Specialist user.
    /// </summary>
    [Table("revision_requests")]
    public class RevisionRequest : ISoftDeletable
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid DeliveryId { get; set; }

        [ForeignKey(nameof(DeliveryId))]
        public virtual ContractDelivery Delivery { get; set; } = null!;

        [Required]
        public string RequestedByClientId { get; set; } = string.Empty;

        [ForeignKey(nameof(RequestedByClientId))]
        public virtual User RequestedByClient { get; set; } = null!;

        [Required]
        [Column(TypeName = "text")]
        public string Reason { get; set; } = string.Empty;

        public DateTime RequestedAt { get; set; } = DateTime.UtcNow;

        [Required]
        public RevisionStatus Status { get; set; } = RevisionStatus.Pending;

        public string? SpecialistId { get; set; }

        [ForeignKey(nameof(SpecialistId))]
        public virtual User? Specialist { get; set; }

        [Column(TypeName = "text")]
        public string? SpecialistDecision { get; set; }

        public DateTime? ResolvedAt { get; set; }

        public bool IsDeleted { get; set; } = false;
    }
}

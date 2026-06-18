using Entities.Common;
using Entities.Enums;
using Entities.Users;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Entities.Project
{
    [Table("contract_specialist_reviews")]
    public class ContractSpecialistReview : ISoftDeletable
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
        public ReviewerType ReviewerType { get; set; } // AI or Human

        public SpecialistReviewStatus Status { get; set; } = SpecialistReviewStatus.Pending;

        // Populated only for Human reviewer
        public string? AssignedSpecialistId { get; set; }
        [ForeignKey(nameof(AssignedSpecialistId))]
        public virtual User? AssignedSpecialist { get; set; }

        // The client's summary of what was expected (used by both AI and Human)
        [Required]
        [Column(TypeName = "text")]
        public string RequirementsSummary { get; set; } = string.Empty;

        // Verdict and reasoning — populated when review is complete
        public ReviewVerdict? Verdict { get; set; }

        [Column(TypeName = "text")]
        public string? ReviewNote { get; set; }

        public DateTime RequestedAt { get; set; } = DateTime.UtcNow;
        public DateTime? CompletedAt { get; set; }

        public bool IsDeleted { get; set; } = false;
    }
}

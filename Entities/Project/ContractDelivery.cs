using Entities.Common;
using Entities.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Entities.Project
{
    /// <summary>
    /// Represents a freelancer's work submission under a contract or milestone.
    /// </summary>
    [Table("contract_deliveries")]
    public class ContractDelivery : ISoftDeletable
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

        public DateTime SubmittedAt { get; set; } = DateTime.UtcNow;

        [Column(TypeName = "text")]
        public string? DeliveryNote { get; set; }

        public DeliveryStatus Status { get; set; } = DeliveryStatus.Pending;

        public DateTime ReviewDeadline { get; set; } = DateTime.UtcNow.AddDays(3);

        public DateTime? CompletedAt { get; set; }

        public bool IsDeleted { get; set; } = false;

        public virtual ICollection<DeliveryAttachment> Attachments { get; set; } = new List<DeliveryAttachment>();
        public virtual ICollection<RevisionRequest> RevisionRequests { get; set; } = new List<RevisionRequest>();
        public virtual ICollection<AdditionalRevisionRequest> AdditionalRevisionRequests { get; set; } = new List<AdditionalRevisionRequest>();
        public virtual ICollection<Dispute> Disputes { get; set; } = new List<Dispute>();
        public virtual ICollection<ContractSpecialistReview> SpecialistReviews { get; set; } = new List<ContractSpecialistReview>();
    }
}

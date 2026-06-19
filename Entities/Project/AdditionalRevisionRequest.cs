using Entities.Common;
using Entities.Enums;
using Entities.Users;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Entities.Project
{
    [Table("additional_revision_requests")]
    public class AdditionalRevisionRequest : ISoftDeletable
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public int ContractId { get; set; }

        [ForeignKey(nameof(ContractId))]
        public virtual Contract Contract { get; set; } = null!;

        [Required]
        public Guid DeliveryId { get; set; }

        [ForeignKey(nameof(DeliveryId))]
        public virtual ContractDelivery Delivery { get; set; } = null!;

        [Required]
        public int RequestedCount { get; set; }

        [Required]
        public string ClientId { get; set; } = string.Empty;

        [ForeignKey(nameof(ClientId))]
        public virtual User Client { get; set; } = null!;

        [Required]
        [Column(TypeName = "text")]
        public string Reason { get; set; } = string.Empty;

        [Required]
        public RequestStatus Status { get; set; } = RequestStatus.Pending;

        public DateTime RequestedAt { get; set; } = DateTime.UtcNow;

        public DateTime? RespondedAt { get; set; }

        public bool IsDeleted { get; set; } = false;
    }
}

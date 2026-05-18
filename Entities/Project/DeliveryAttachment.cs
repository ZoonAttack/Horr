using Entities.Common;
using Entities.Enums;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Entities.Project
{
    /// <summary>
    /// Represents files or links attached to a delivery. Supports both legacy WorkDelivery and new ContractDelivery.
    /// </summary>
    [Table("delivery_attachments")]
    public class DeliveryAttachment : ISoftDeletable
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        // ── New ContractDelivery Relation ────────────────────────
        public Guid? DeliveryId { get; set; }

        [ForeignKey(nameof(DeliveryId))]
        public virtual ContractDelivery? Delivery { get; set; }

        // ── Legacy WorkDelivery Relation ─────────────────────────
        public int? WorkDeliveryId { get; set; }

        [ForeignKey(nameof(WorkDeliveryId))]
        public virtual WorkDelivery? WorkDelivery { get; set; }

        // ── New Fields ──────────────────────────────────────────
        public AttachmentType Type { get; set; } = AttachmentType.File;

        [MaxLength(255)]
        public string? FileName { get; set; }

        [MaxLength(500)]
        public string? StoragePath { get; set; }

        [MaxLength(2048)]
        public string? Url { get; set; }

        // ── Legacy Fields (to prevent build errors) ──────────────
        [MaxLength(2048)]
        public string FileUrl { get; set; } = string.Empty;

        [MaxLength(100)]
        public string FileType { get; set; } = string.Empty;

        public DateTime UploadedAt { get; set; } = DateTime.UtcNow;

        // ── ISoftDeletable ────────────────────────────────────────
        public bool IsDeleted { get; set; } = false;
    }
}

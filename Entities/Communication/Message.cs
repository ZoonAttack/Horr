using Entities.Common;
using Entities.Enums;
using Entities.Users;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Entities.Communication
{
    /// <summary>
    /// Represents an individual message within a Chat.
    /// Supports soft-delete via ISoftDeletable.
    /// </summary>
    [Table("messages")]
    public class Message : ISoftDeletable
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public string Id { get; set; } = string.Empty;

        [Required]
        public string ChatId { get; set; } = string.Empty;

        [ForeignKey(nameof(ChatId))]
        public virtual Chat Chat { get; set; } = null!;

        [Required]
        public string SenderId { get; set; } = string.Empty;

        [ForeignKey(nameof(SenderId))]
        public virtual User Sender { get; set; } = null!;

        [Required]
        [Column(TypeName = "text")]
        public string Body { get; set; } = string.Empty;

        public MessageStatus Status { get; set; } = MessageStatus.Unread;

        public DateTime SentAt { get; set; } = DateTime.UtcNow;

        // ── Soft Delete (ISoftDeletable) ──────────────────────────────────
        public bool IsDeleted { get; set; } = false;

        public MessageType Type { get; set; } = MessageType.Text;

        [Column(TypeName = "text")]
        public string? TextContent { get; set; }

        public string? FileUrl { get; set; }

        public string? FileName { get; set; }

        public long? FileSizeBytes { get; set; }

        // ── Navigation Properties ─────────────────────────────────────────
        public virtual ICollection<Attachment> Attachments { get; set; } = new List<Attachment>();
        public virtual ICollection<Entities.Project.Delivery> Deliveries { get; set; } = new List<Entities.Project.Delivery>();
    }
}

using Entities.Common;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Entities.Communication
{
    /// <summary>
    /// Represents a direct-message conversation between two or more users.
    /// Supports soft-delete via ISoftDeletable — hard-deletes are forbidden.
    /// A Global Query Filter on IsDeleted is applied in AppDbContext.
    /// </summary>
    [Table("conversations")]
    public class Conversation : ISoftDeletable
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public string Id { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // ── Soft Delete (ISoftDeletable) ──────────────────────────────────
        public bool IsDeleted { get; set; } = false;

        // ── Navigation Properties ─────────────────────────────────────────
        public virtual ICollection<ConversationParticipant> Participants { get; set; }
            = new List<ConversationParticipant>();

        public virtual ICollection<Message> Messages { get; set; }
            = new List<Message>();
    }
}

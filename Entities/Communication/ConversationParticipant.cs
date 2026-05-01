using Entities.Users;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Entities.Communication
{
    /// <summary>
    /// Join entity for Conversations and Users with a composite primary key.
    /// </summary>
    [Table("conversation_participants")]
    public class ConversationParticipant
    {
        [Required]
        public string ConversationId { get; set; } = string.Empty;

        [ForeignKey(nameof(ConversationId))]
        public virtual Conversation Conversation { get; set; } = null!;

        [Required]
        public string UserId { get; set; } = string.Empty;

        [ForeignKey(nameof(UserId))]
        public virtual User User { get; set; } = null!;

        public DateTime JoinedAt { get; set; } = DateTime.UtcNow;
    }
}

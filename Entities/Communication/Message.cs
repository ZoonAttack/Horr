using Entities.Project;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

 

namespace Entities.Communication
{
    /// <summary>
    /// An individual message within a Chat.
    /// </summary>
    [Table("messages")]
    [Index(nameof(ChatId))]
    [Index(nameof(SenderId))]
    [Index(nameof(IsRead))]
    public class Message
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }

        [Required]
        [ForeignKey("Chat")]
        public long ChatId { get; set; }
        public virtual Chat Chat { get; set; }

        [Required]
        [ForeignKey("Sender")]
        public long SenderId { get; set; }
        public virtual Entities.User.User Sender { get; set; }

        [Column(TypeName = "text")]
        public string Content { get; set; }

        // File Attachment Support (PCR-02)
        [MaxLength(255)]
        public string AttachmentUrl { get; set; }

        [MaxLength(50)]
        public string AttachmentType { get; set; }

        public bool IsRead { get; set; } = false;

        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public DateTime SentAt { get; set; }

        // --- Navigation Properties ---
        public virtual ICollection<Delivery> Deliveries { get; set; } = new List<Delivery>();
    }
}

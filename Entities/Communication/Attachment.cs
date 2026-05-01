using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Entities.Communication
{
    /// <summary>
    /// Represents a file attachment linked to a Message.
    /// </summary>
    [Table("attachments")]
    public class Attachment
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public string Id { get; set; } = string.Empty;

        [Required]
        public string MessageId { get; set; } = string.Empty;

        [ForeignKey(nameof(MessageId))]
        public virtual Message Message { get; set; } = null!;

        [Required]
        [MaxLength(2000)]
        public string FileUrl { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string FileType { get; set; } = string.Empty;

        public DateTime UploadedAt { get; set; } = DateTime.UtcNow;
    }
}

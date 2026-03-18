using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Entities.Project
{
    /// <summary>
    /// Represents an attachment for a work delivery.
    /// </summary>
    [Table("delivery_attachments")]
    public class DeliveryAttachment
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int WorkDeliveryId { get; set; }

        [ForeignKey(nameof(WorkDeliveryId))]
        public virtual WorkDelivery WorkDelivery { get; set; } = null!;

        [Required]
        [MaxLength(2048)]
        public string FileUrl { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string FileType { get; set; } = string.Empty;

        public DateTime UploadedAt { get; set; } = DateTime.UtcNow;
    }
}

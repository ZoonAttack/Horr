using Entities.Communication;
using Entities.Enums;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Entities.Project
{
    /// <summary>
    /// Represents a file delivery for a project, linked to a message.
    /// </summary>
    [Table("deliveries")]
    [Index(nameof(ProjectId))]
    [Index(nameof(Status))]
    public class Delivery
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }

        [Required]
        [ForeignKey("Project")]
        public string ProjectId { get; set; }
        public virtual ClientProject Project { get; set; }

        [Required]
        [ForeignKey("Message")]
        public long MessageId { get; set; }
        public virtual Message Message { get; set; }

        [ForeignKey("Proposal")]
        public long? ProposalId { get; set; }
        public virtual Proposal Proposal { get; set; }

        [Required]
        [MaxLength(255)]
        public string FileUrl { get; set; }

        public DeliveryStatus Status { get; set; } = DeliveryStatus.Pending;

        [Required]
        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public DateTime DeliveredAt { get; set; }

        public DateTime? ReviewedAt { get; set; }

        [Column(TypeName = "text")]
        public string ReviewNotes { get; set; }
    }
}

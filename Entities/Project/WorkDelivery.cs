using Entities.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Entities.Project
{
    /// <summary>
    /// Represents a work delivery submission by a freelancer under a Contract.
    /// </summary>
    [Table("work_deliveries")]
    public class WorkDelivery
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int ContractId { get; set; }

        [ForeignKey(nameof(ContractId))]
        public virtual Contract Contract { get; set; } = null!;

        [Required]
        [Column(TypeName = "text")]
        public string Note { get; set; } = string.Empty;

        public DateTime SubmittedAt { get; set; } = DateTime.UtcNow;

        public ActionStatus ActionStatus { get; set; } = ActionStatus.NeedsAttention;

        // ── Navigation ────────────────────────────────────────────────────
        public virtual ICollection<DeliveryAttachment> Attachments { get; set; } = new List<DeliveryAttachment>();
    }
}

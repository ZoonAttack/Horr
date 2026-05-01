using Entities.Common;
using Entities.Enums;
using Entities.Users;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Entities.Payment
{
    [Table("deposit_requests")]
    public class DepositRequest : ISoftDeletable
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public string Id { get; set; }

        [Required]
        public string ClientId { get; set; }

        [ForeignKey(nameof(ClientId))]
        public virtual User Client { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Amount { get; set; }

        [Required]
        [MaxLength(100)]
        public string ReceiptNumber { get; set; }

        [Required]
        public string ReceiptPhotoUrl { get; set; }

        public DepositStatus Status { get; set; } = DepositStatus.Pending;

        public string? AdminNote { get; set; }

        public DateTime SubmittedAt { get; set; } = DateTime.UtcNow;

        public DateTime? ReviewedAt { get; set; }

        public bool IsDeleted { get; set; } = false;
    }
}

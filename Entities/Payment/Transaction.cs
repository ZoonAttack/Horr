using Entities.Enums;
using Entities.Users;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Entities.Payment
{
    [Table("transactions")]
    public class Transaction
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public string Id { get; set; }

        [Required]
        public string UserId { get; set; }

        [ForeignKey(nameof(UserId))]
        public virtual User User { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Amount { get; set; }

        public TransactionDirection Direction { get; set; }
        public TransactionType Type { get; set; }

        [Required]
        public string Description { get; set; }

        public string? RelatedDepositRequestId { get; set; }

        [ForeignKey(nameof(RelatedDepositRequestId))]
        public virtual DepositRequest? RelatedDepositRequest { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // --- Navigation Properties for existing entities ---
        public virtual ICollection<PaymentTransaction> PaymentTransactions { get; set; } = new List<PaymentTransaction>();
        public virtual ICollection<FundRequest> FundRequests { get; set; } = new List<FundRequest>();
    }
}

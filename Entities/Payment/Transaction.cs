using Entities.Enums;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Entities.Payment
{
    /// <summary>
    /// A ledger entry for any movement of funds between wallets.
    /// </summary>
    [Table("transactions")]
    [Index(nameof(SenderWalletId))]
    [Index(nameof(ReceiverWalletId))]
    [Index(nameof(Status))]
    [Index(nameof(TransactionType))]
    public class Transaction
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }

        [ForeignKey("SenderWallet")]
        public long? SenderWalletId { get; set; }

        [ForeignKey("ReceiverWallet")]
        public long? ReceiverWalletId { get; set; }

        [Required]
        [Column(TypeName = "decimal(10,2)")]
        [Range(0.01, (double)decimal.MaxValue)]
        public decimal Amount { get; set; }

        [Required]
        public string? TransactionType { get; set; }

        public string? Status { get; set; } = TransactionStatus.Pending.ToString();

        [Column(TypeName = "text")]
        public string Description { get; set; }

        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public DateTime CreatedAt { get; set; }

        public DateTime? CompletedAt { get; set; }

        // --- Navigation Properties ---
        public virtual Wallet SenderWallet { get; set; }
        public virtual Wallet ReceiverWallet { get; set; }

        public virtual ICollection<PaymentTransaction> PaymentTransactions { get; set; } = new List<PaymentTransaction>();
    }
}

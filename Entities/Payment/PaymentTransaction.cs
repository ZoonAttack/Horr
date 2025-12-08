using Entities.Enums;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Entities.Payment
{
    /// <summary>
    /// Junction table linking a high-level Payment to one or more
    /// low-level wallet Transactions.
    /// </summary>
    [Table("payment_transactions")]
    [Index(nameof(PaymentId))]
    [Index(nameof(TransactionId))]
    public class PaymentTransaction
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public string Id { get; set; }

        [Required]
        [ForeignKey("Payment")]
        public string PaymentId { get; set; }
        public virtual Payment Payment { get; set; }

        [Required]
        [ForeignKey("Transaction")]
        public string TransactionId { get; set; }
        public virtual Transaction Transaction { get; set; }

        [Required]
        public PaymentTransactionRole Role { get; set; }

        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public DateTime CreatedAt { get; set; }
    }
}

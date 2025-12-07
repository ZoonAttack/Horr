using Entities.Enums;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities.Payment
{
    [Table("fund_requests")]
    [Index(nameof(UserId))]
    [Index(nameof(Status))]
    public class FundRequest
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }

        [Required]
        [ForeignKey("User")]
        public long UserId { get; set; }
        public virtual Entities.User.User User { get; set; }

        [Required]
        [ForeignKey("PaymentMethod")]
        public long PaymentMethodId { get; set; }
        public virtual PaymentMethod PaymentMethod { get; set; }

        [Required]
        [Column(TypeName = "decimal(10,2)")]
        public decimal Amount { get; set; }

        [Required]
        [MaxLength(100)]
        public string ClientTransactionReference { get; set; } // The ID user gets from InstaPay/Bank

        public string? Status { get; set; } = RequestStatus.Pending.ToString();

        public long? ApprovedByUserId { get; set; } // Admin ID
        public DateTime? ProcessedAt { get; set; }

        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public DateTime RequestedAt { get; set; }

        // --- Navigation Properties ---
        public long? TransactionId { get; set; } // The resulting ledger transaction
        public virtual Transaction Transaction { get; set; }
    }
}

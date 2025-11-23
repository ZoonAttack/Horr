using Entities.Enums;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


namespace Entities.Payment
{
    [Table("user_payment_methods")]
    [Index(nameof(UserId))]
    public class PaymentMethod
    {
  
            [Key]
            [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
            public long Id { get; set; }

            [Required]
            [ForeignKey("User")]
            public long UserId { get; set; }
            public virtual Entities.User.User User { get; set; }

            [Required]
            [MaxLength(50)]
            public string? MethodName { get; set; } // e.g., "InstaPay", "Vodafone Cash"

            [Required]
            [MaxLength(100)]
            public string AccountIdentifier { get; set; } // e.g., InstaPay ID or E-Wallet Phone Number

            [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
            public DateTime CreatedAt { get; set; }

            // --- Navigation Properties ---
            public virtual ICollection<FundRequest> FundRequests { get; set; } = new List<FundRequest>();
            public virtual ICollection<WithdrawalRequest> WithdrawalRequests { get; set; } = new List<WithdrawalRequest>();
    }
}


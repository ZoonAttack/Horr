using Entities.Common;
using Entities.Enums;
using Entities.Users;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Entities.Payment
{
    [Table("withdrawal_requests")]
    public class WithdrawalRequest : ISoftDeletable
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public string Id { get; set; }

        [Required]
        public string FreelancerId { get; set; }

        [ForeignKey(nameof(FreelancerId))]
        public virtual User Freelancer { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Amount { get; set; }

        public WithdrawalMethod Method { get; set; }

        public string? InstapayUsername { get; set; }

        public string? BankAccountDetails { get; set; }

        public string? EWalletNumber { get; set; }

        public WithdrawalStatus Status { get; set; } = WithdrawalStatus.Pending;

        public string? AdminNote { get; set; }

        public DateTime SubmittedAt { get; set; } = DateTime.UtcNow;

        public DateTime? ReviewedAt { get; set; }

        public bool IsDeleted { get; set; } = false;
    }
}

using Entities.Users;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Entities.Payment
{
    [Table("wallet_balances")]
    public class WalletBalance
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public string Id { get; set; }

        [Required]
        public string UserId { get; set; }

        [ForeignKey(nameof(UserId))]
        public virtual User User { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal BalanceEGP { get; set; } = 0;

        public DateTime LastUpdatedAt { get; set; } = DateTime.UtcNow;
    }
}

using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Entities.Payment
{
    /// <summary>
    /// Represents a user's internal wallet for holding funds.
    /// </summary>
    [Table("wallets")]
    [Index(nameof(UserId), IsUnique = true)]
    public class Wallet
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }

        [Required]
        [ForeignKey("User")]
        public string UserId { get; set; }
        public virtual Entities.User.User User { get; set; }

        [Required]
        [Column(TypeName = "decimal(10,2)")]
        [Range(0, (double)decimal.MaxValue)]
        public decimal Balance { get; set; } = 0;

        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public DateTime CreatedAt { get; set; }

        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public DateTime UpdatedAt { get; set; }

        // --- Navigation Properties ---
        [InverseProperty("SenderWallet")]
        public virtual ICollection<Transaction> SentTransactions { get; set; } = new List<Transaction>();

        [InverseProperty("ReceiverWallet")]
        public virtual ICollection<Transaction> ReceivedTransactions { get; set; } = new List<Transaction>();
    }
}

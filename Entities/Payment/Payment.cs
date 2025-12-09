using Entities.Enums;
using Entities.Project;
using Entities.User;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Entities.Payment
{
    /// <summary>
    /// A high-level payment record related to a project.
    /// </summary>
    [Table("payments")]
    [Index(nameof(ProjectId))]
    [Index(nameof(FreelancerId))]
    [Index(nameof(Status))]
    [Index(nameof(PaymentType))]
    public class Payment
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }

        [Required]
        [ForeignKey("Project")]
        public string ProjectId { get; set; }
        public virtual ClientProject Project { get; set; }

        [ForeignKey("Freelancer")]
        public long? FreelancerId { get; set; }
        public virtual Freelancer Freelancer { get; set; }

        [Required]
        [Column(TypeName = "decimal(10,2)")]
        [Range(0.01, (double)decimal.MaxValue)]
        public decimal Amount { get; set; }

        [Required]
        public PaymentType PaymentType { get; set; }

        public PaymentStatus Status { get; set; } = PaymentStatus.Pending;

        [Column(TypeName = "decimal(10,2)")]
        public decimal? PlatformCommission { get; set; }

        [Required]
        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public DateTime CreatedAt { get; set; }

        public DateTime? ReleasedAt { get; set; }

        // --- Navigation Properties ---
        public virtual ICollection<PaymentTransaction> PaymentTransactions { get; set; } = new List<PaymentTransaction>();
    }
}

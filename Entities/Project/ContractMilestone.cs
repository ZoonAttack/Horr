using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Entities.Project
{
    [Table("contract_milestones")]
    public class ContractMilestone
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int ContractId { get; set; }

        [ForeignKey(nameof(ContractId))]
        public virtual Contract Contract { get; set; } = null!;

        [Required]
        [MaxLength(200)]
        public string Description { get; set; } = string.Empty;

        [Column(TypeName = "decimal(18,2)")]
        public decimal Amount { get; set; }

        public DateTime DueDate { get; set; }

        // Optionally track milestone status (e.g., Pending, Funded, Completed, Paid)
        // Leaving it simple for now as requested.
    }
}

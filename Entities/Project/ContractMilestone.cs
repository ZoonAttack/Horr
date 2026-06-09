using Entities.Common;
using Entities.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Entities.Project
{
    [Table("contract_milestones")]
    public class ContractMilestone : ISoftDeletable
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public int ContractId { get; set; }

        [ForeignKey(nameof(ContractId))]
        public virtual Contract Contract { get; set; } = null!;

        [Required]
        [MaxLength(200)]
        public string Title { get; set; } = string.Empty;

        public string? Description { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Amount { get; set; }

        public DateTime DueDate { get; set; }

        public MilestoneStatus Status { get; set; } = MilestoneStatus.Unfunded;

        public DateTime? FundedAt { get; set; }

        public DateTime? ReleasedAt { get; set; }

        public bool IsDeleted { get; set; } = false;
    }
}

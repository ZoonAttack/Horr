using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Entities.Project
{
    [Table("proposal_terms")]
    public class ProposalTerm
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int ProposalId { get; set; }

        [ForeignKey(nameof(ProposalId))]
        public virtual Proposal Proposal { get; set; } = null!;

        [Required]
        [MaxLength(200)]
        public string MilestoneTitle { get; set; } = string.Empty;

        [Column(TypeName = "decimal(18,2)")]
        public decimal Amount { get; set; }

        public DateTime DueDate { get; set; }
    }
}

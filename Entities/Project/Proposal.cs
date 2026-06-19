using Entities.Enums;
using Entities.Users;
using Entities.Common;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Entities.Project
{
    [Table("proposals")]
    public class Proposal : ISoftDeletable
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string JobPostId { get; set; } = string.Empty;

        [ForeignKey(nameof(JobPostId))]
        public virtual JobPost JobPost { get; set; } = null!;

        [Required]
        public string FreelancerId { get; set; } = string.Empty;

        [ForeignKey(nameof(FreelancerId))]
        public virtual Freelancer Freelancer { get; set; } = null!;

        public SubmitAsType SubmitAsType { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal BidRate { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal HORRFee { get; set; }

        [Required]
        [MaxLength(2000)]
        public string CoverLetter { get; set; } = string.Empty;

        public ProposalStatus Status { get; set; } = ProposalStatus.Active;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public int MaxRevisions { get; set; }

        public int DurationDays { get; set; }

        // Soft Delete
        public bool IsDeleted { get; set; }

        public virtual ICollection<ProposalTerm> Terms { get; set; } = new List<ProposalTerm>();
        public virtual ICollection<Delivery> Deliveries { get; set; } = new List<Delivery>();
    }
}

using Entities.Enums;
using Entities.Users;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Entities.Project
{
    [Table("job_invitations")]
    public class JobInvitation
    {
        [Key]
        public string Id { get; set; } = Guid.NewGuid().ToString();

        [Required]
        public string JobPostId { get; set; } = string.Empty;

        [ForeignKey(nameof(JobPostId))]
        public virtual JobPost JobPost { get; set; } = null!;

        [Required]
        public string FreelancerId { get; set; } = string.Empty;

        [ForeignKey(nameof(FreelancerId))]
        public virtual User Freelancer { get; set; } = null!;

        [Required]
        public string ClientId { get; set; } = string.Empty;

        [ForeignKey(nameof(ClientId))]
        public virtual User Client { get; set; } = null!;

        public string? Message { get; set; }

        public InvitationStatus Status { get; set; } = InvitationStatus.Pending;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? RespondedAt { get; set; }

        public int? ProposalId { get; set; }

        [ForeignKey(nameof(ProposalId))]
        public virtual Proposal? Proposal { get; set; }
    }
}

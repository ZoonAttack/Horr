using Entities.Communication;
using Entities.Enums;
using Entities.Marketplace;
using Entities.Review;
using Entities.Users;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Entities.Project
{
    /// <summary>
    /// Represents a project posted by a client.
    /// </summary>
    [Table("client_projects")]
    [Index(nameof(ClientId))]
    [Index(nameof(Status))]
    [Index(nameof(IsDeleted))]
    [Index(nameof(Deadline))]
    public class ClientProject
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public string Id { get; set; }

        [Required]
        [ForeignKey("Client")]
        public string ClientId { get; set; }
        public virtual Client Client { get; set; }

        public string? AcceptedProposalId { get; set; }

        [Required]
        [MaxLength(255)]
        public string Title { get; set; }

        [Column(TypeName = "text")]
        public string Description { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal? Budget { get; set; }

        public DateTime? Deadline { get; set; }

        [Required]
        public ProjectStatus Status { get; set; } = ProjectStatus.Open;

        // Soft Delete
        public bool IsDeleted { get; set; } = false;
        public DateTime? DeletedAt { get; set; }

        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public DateTime CreatedAt { get; set; }

        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public DateTime UpdatedAt { get; set; }

        // --- Navigation Properties ---
        [ForeignKey("AcceptedProposalId")]
        public virtual Proposal AcceptedProposal { get; set; }

        [InverseProperty("Project")]
        public virtual ICollection<Proposal> Proposals { get; set; } = new List<Proposal>();

        public virtual ICollection<Order> Orders { get; set; } = new List<Order>();
        public virtual ICollection<Chat> Chats { get; set; } = new List<Chat>();
        public virtual ICollection<Delivery> Deliveries { get; set; } = new List<Delivery>();
        public virtual ICollection<Entities.Payment.Payment> Payments { get; set; } = new List<Entities.Payment.Payment>();
        public virtual ICollection<Entities.Review.Review> Reviews { get; set; } = new List<Entities.Review.Review>();
        public virtual ICollection<SpecialistReviewRequest> SpecialistReviewRequests { get; set; } = new List<SpecialistReviewRequest>();
        public virtual ICollection<Contract> Contracts { get; set; } = new List<Contract>();
    }
}

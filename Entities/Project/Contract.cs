using Entities.Enums;
using Entities.Users;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Entities.Project
{
    /// <summary>
    /// Represents a formal contract for a project between a client and freelancer.
    /// </summary>
    [Table("contracts")]
    [Index(nameof(ProjectId))]
    [Index(nameof(ClientId))]
    [Index(nameof(FreelancerId))]
    [Index(nameof(Status))]
    public class Contract
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public string Id { get; set; }

        [Required]
        [ForeignKey("Project")]
        public string ProjectId { get; set; }
        public virtual ClientProject Project { get; set; }

        [Required]
        [ForeignKey("Client")]
        public string ClientId { get; set; }
        public virtual Client Client { get; set; }

        [Required]
        [ForeignKey("Freelancer")]
        public string FreelancerId { get; set; }
        public virtual Freelancer Freelancer { get; set; }

        [Column(TypeName = "text")]
        public string Terms { get; set; }

        public ContractStatus Status { get; set; } = ContractStatus.Draft;

        public DateTime? SignedAt { get; set; }

        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public DateTime CreatedAt { get; set; }

        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public DateTime UpdatedAt { get; set; }
    }
}

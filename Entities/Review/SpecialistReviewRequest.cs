using Entities.Enums;
using Entities.Project;
using Entities.User;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Entities.Review
{
    /// <summary>
    /// A request for a specialist to review a project.
    /// </summary>
    [Table("specialist_review_requests")]
    [Index(nameof(ClientId))]
    [Index(nameof(ProjectId))]
    [Index(nameof(SpecialistId))]
    [Index(nameof(Status))]
    public class SpecialistReviewRequest
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }

        [Required]
        [ForeignKey("Client")]
        public string ClientId { get; set; }
        public virtual Client Client { get; set; }

        [Required]
        [ForeignKey("Project")]
        public string ProjectId { get; set; }
        public virtual ClientProject Project { get; set; }

        [ForeignKey("Specialist")]
        public string? SpecialistId { get; set; }
        public virtual Entities.User.User Specialist { get; set; }

        [Required]
        [Column(TypeName = "decimal(10,2)")]
        public decimal ReviewFee { get; set; }

        public SpecialistReviewStatus Status { get; set; } = SpecialistReviewStatus.Pending;

        [Column(TypeName = "text")]
        public string ReviewNotes { get; set; }

        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public DateTime RequestedAt { get; set; }

        public DateTime? CompletedAt { get; set; }
    }
}

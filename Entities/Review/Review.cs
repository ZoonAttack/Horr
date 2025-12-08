using Entities.Marketplace;
using Entities.Project;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Entities.Review
{
    /// <summary>
    /// A review left by one user for another, tied to a project or order.
    /// </summary>
    [Table("reviews")]
    [Index(nameof(ReviewerId))]
    [Index(nameof(RevieweeId))]
    [Index(nameof(ProjectId))]
    [Index(nameof(OrderId))]
    [Index(nameof(IsDeleted))]
    public class Review
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }

        [Required]
        [ForeignKey("Reviewer")]
        public string ReviewerId { get; set; }

        [Required]
        [ForeignKey("Reviewee")]
        public string RevieweeId { get; set; }

        [ForeignKey("Project")]
        public long? ProjectId { get; set; }
        public virtual ClientProject Project { get; set; }

        [ForeignKey("Order")]
        public long? OrderId { get; set; }
        public virtual Order Order { get; set; }

        [Required]
        [Range(1, 5)]
        public int Rating { get; set; }

        [Column(TypeName = "text")]
        public string Comment { get; set; }

        // Soft Delete
        public bool IsDeleted { get; set; } = false;
        public DateTime? DeletedAt { get; set; }

        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public DateTime CreatedAt { get; set; }

        // --- Navigation Properties ---
        public virtual Entities.User.User Reviewer { get; set; }
        public virtual Entities.User.User Reviewee { get; set; }
    }
}

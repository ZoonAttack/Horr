using Entities.Enums;
using Entities.Project;
using Entities.User;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Entities.Marketplace
{
    /// <summary>
    /// Represents a formal order for either a project or a service.
    /// </summary>
    [Table("orders")]
    [Index(nameof(ClientId))]
    [Index(nameof(FreelancerId))]
    [Index(nameof(Status))]
    [Index(nameof(OrderType))]
    public class Order
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public string Id { get; set; }

        [Required]
        [ForeignKey("Client")]
        public string ClientId { get; set; }
        public virtual Client Client { get; set; }

        [Required]
        [ForeignKey("Freelancer")]
        public string FreelancerId { get; set; }
        public virtual Freelancer Freelancer { get; set; }

        [ForeignKey("Service")]
        public long? ServiceId { get; set; }
        public virtual Service Service { get; set; }

        [ForeignKey("Project")]
        public long? ProjectId { get; set; }
        public virtual ClientProject Project { get; set; }

        [Required]
        public OrderType OrderType { get; set; }

        [Required]
        [Column(TypeName = "decimal(10,2)")]
        public decimal Amount { get; set; }

        public OrderStatus Status { get; set; } = OrderStatus.Pending;

        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public DateTime CreatedAt { get; set; }

        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public DateTime UpdatedAt { get; set; }

        // --- Navigation Properties ---
        public virtual ICollection<Entities.Review.Review> Reviews { get; set; } = new List<Entities.Review.Review>();
    }
}

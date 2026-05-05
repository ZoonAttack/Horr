using Entities.Users;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Entities.Users.FreelancerHelpers
{
    [Table("portfolio_items")]
    public class PortfolioItem
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public string Id { get; set; }

        [Required]
        public string FreelancerId { get; set; }

        [Required]
        [MaxLength(150)]
        public string Title { get; set; }

        [Required]
        [MaxLength(1000)]
        public string Description { get; set; }

        [MaxLength(150)]
        public string? Role { get; set; }

        [MaxLength(500)]
        public string? VisitLink { get; set; }

        public string? ThumbnailUrl { get; set; }

        public bool IsDeleted { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime UpdatedAt { get; set; }

        // Navigation
        [ForeignKey("FreelancerId")]
        public virtual Freelancer Freelancer { get; set; }
        
        public virtual ICollection<PortfolioMedia> Media { get; set; } = new List<PortfolioMedia>();
    }
}

using Entities.Users;
using Entities.Common;
using Entities.Enums;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Collections.Generic;
using System;

namespace Entities.Marketplace
{
    /// <summary>
    /// A pre-defined service package offered by a freelancer.
    /// </summary>
    [Table("services")]
    [Index(nameof(FreelancerId))]
    [Index(nameof(IsActive))]
    [Index(nameof(IsDeleted))]
    public class ServiceCatalogItem : ISoftDeletable
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public string Id { get; set; }

        [Required]
        [ForeignKey("Freelancer")]
        public string FreelancerId { get; set; }
        public virtual Freelancer Freelancer { get; set; }

        [Required]
        [MaxLength(255)]
        public string Title { get; set; }

        [Required]
        [MinLength(120)]
        [Column(TypeName = "text")]
        public string Description { get; set; }

        public string? CoverImageUrl { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal? Price { get; set; }

        [MaxLength(50)]
        public string? DeliveryTime { get; set; }

        public bool IsActive { get; set; } = true;
        public ServiceStatus Status { get; set; } = ServiceStatus.UnderReview;

        // Soft Delete
        public bool IsDeleted { get; set; } = false;
        public DateTime? DeletedAt { get; set; }

        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public DateTime CreatedAt { get; set; }

        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public DateTime UpdatedAt { get; set; }

        // --- Navigation Properties ---
        public virtual ServicePricing Pricing { get; set; }
        public virtual ICollection<ServiceGalleryFile> GalleryFiles { get; set; } = new List<ServiceGalleryFile>();
        public virtual ICollection<ServiceRequirement> Requirements { get; set; } = new List<ServiceRequirement>();
        public virtual ICollection<ServiceStep> Steps { get; set; } = new List<ServiceStep>();
        public virtual ICollection<ServiceFaq> Faqs { get; set; } = new List<ServiceFaq>();
        public virtual ICollection<ServiceAttribute> Attributes { get; set; } = new List<ServiceAttribute>();
        public virtual ICollection<Order> Orders { get; set; } = new List<Order>();
    }
}

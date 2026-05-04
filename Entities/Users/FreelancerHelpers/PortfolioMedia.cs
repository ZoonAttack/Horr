using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Entities.Users.FreelancerHelpers
{
    [Table("portfolio_media")]
    public class PortfolioMedia
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public string Id { get; set; }

        [Required]
        public string PortfolioItemId { get; set; }

        [Required]
        public string FileUrl { get; set; }

        [Required]
        public string FileType { get; set; }    // "Image" | "Video"

        public DateTime UploadedAt { get; set; }

        // Navigation
        [ForeignKey("PortfolioItemId")]
        public virtual PortfolioItem PortfolioItem { get; set; }
    }
}

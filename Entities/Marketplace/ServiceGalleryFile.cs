using Entities.Enums;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Entities.Marketplace
{
    [Table("service_gallery_files")]
    public class ServiceGalleryFile
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public string Id { get; set; }

        [Required]
        [ForeignKey("Service")]
        public string ServiceId { get; set; }
        public virtual ServiceCatalogItem Service { get; set; }

        [Required]
        public string FileUrl { get; set; }

        [Required]
        public ServiceGalleryFileType FileType { get; set; }

        public bool IsCover { get; set; }

        public DateTime UploadedAt { get; set; } = DateTime.UtcNow;
    }
}

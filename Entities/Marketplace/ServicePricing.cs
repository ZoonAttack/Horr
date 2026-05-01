using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Entities.Marketplace
{
    [Table("service_pricings")]
    public class ServicePricing
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public string Id { get; set; }

        [Required]
        [ForeignKey("Service")]
        public string ServiceId { get; set; }
        public virtual ServiceCatalogItem Service { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal PriceFrom { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal? PriceTo { get; set; }

        [Required]
        public int DeliveryDays { get; set; }

        [Required]
        public int RevisionsIncluded { get; set; }
    }
}

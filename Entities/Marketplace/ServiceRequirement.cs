using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Entities.Marketplace
{
    [Table("service_requirements")]
    public class ServiceRequirement
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public string Id { get; set; }

        [Required]
        [ForeignKey("Service")]
        public string ServiceId { get; set; }
        public virtual ServiceCatalogItem Service { get; set; }

        [Required]
        [MinLength(10)]
        [MaxLength(250)]
        public string Question { get; set; }

        public bool IsRequired { get; set; }
    }
}

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Entities.Marketplace
{
    [Table("service_steps")]
    public class ServiceStep
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public string Id { get; set; }

        [Required]
        [ForeignKey("Service")]
        public string ServiceId { get; set; }
        public virtual ServiceCatalogItem Service { get; set; }

        [Required]
        public int StepNumber { get; set; }

        [Required]
        [MinLength(3)]
        [MaxLength(75)]
        public string Title { get; set; }

        [MaxLength(250)]
        public string? Description { get; set; }
    }
}

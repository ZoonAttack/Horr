using System.ComponentModel.DataAnnotations;

namespace ServiceContracts.DTOs.Services
{
    /// <summary>
    /// DTO for updating existing Service details.
    /// </summary>
    public class ServiceUpdateDTO
    {
        [MaxLength(255)]
        public string Title { get; set; }

        public string Description { get; set; }

        public decimal? Price { get; set; }

        [MaxLength(50)]
        public string DeliveryTime { get; set; }

        public bool IsActive { get; set; }
    }
}

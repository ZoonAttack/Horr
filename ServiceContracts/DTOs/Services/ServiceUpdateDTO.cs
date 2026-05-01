using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ServiceContracts.DTOs.Services
{
    /// <summary>
    /// DTO for updating existing Service details.
    /// </summary>
    public class ServiceUpdateDTO
    {
        public string Id { get; set; }
        public string FreelancerId { get; set; }

        [MaxLength(255)]
        public string Title { get; set; }

        public string Description { get; set; }

        public string? CoverImageUrl { get; set; }

        public decimal? Price { get; set; }

        [MaxLength(50)]
        public string? DeliveryTime { get; set; }

        public bool IsActive { get; set; }

        public ServicePricingDto Pricing { get; set; }
        public List<ServiceGalleryFileDto> GalleryFiles { get; set; } = new List<ServiceGalleryFileDto>();
        public List<ServiceRequirementDto> Requirements { get; set; } = new List<ServiceRequirementDto>();
        public List<ServiceStepDto> Steps { get; set; } = new List<ServiceStepDto>();
        public List<ServiceFaqDto> Faqs { get; set; } = new List<ServiceFaqDto>();
        public List<ServiceAttributeDto> Attributes { get; set; } = new List<ServiceAttributeDto>();
    }
}

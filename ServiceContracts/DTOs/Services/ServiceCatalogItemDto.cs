using System;
using System.Collections.Generic;

namespace ServiceContracts.DTOs.Services
{
    public class ServiceCatalogItemDto
    {
        public string Id { get; set; }
        public string FreelancerId { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string? CoverImageUrl { get; set; }
        public decimal? Price { get; set; }
        public string? DeliveryTime { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        public ServicePricingDto Pricing { get; set; }
        public IEnumerable<ServiceGalleryFileDto> GalleryFiles { get; set; } = new List<ServiceGalleryFileDto>();
        public IEnumerable<ServiceRequirementDto> Requirements { get; set; } = new List<ServiceRequirementDto>();
        public IEnumerable<ServiceStepDto> Steps { get; set; } = new List<ServiceStepDto>();
        public IEnumerable<ServiceFaqDto> Faqs { get; set; } = new List<ServiceFaqDto>();
        public IEnumerable<ServiceAttributeDto> Attributes { get; set; } = new List<ServiceAttributeDto>();
    }
}

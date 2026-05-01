using System;
using System.Collections.Generic;

namespace ServiceContracts.DTOs.Services
{
    public class ServiceCreateDTO
    {
        public string FreelancerId { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string? CoverImageUrl { get; set; }
        public decimal? Price { get; set; }
        public string? DeliveryTime { get; set; }

        public ServicePricingDto Pricing { get; set; }
        public List<ServiceGalleryFileDto> GalleryFiles { get; set; } = new List<ServiceGalleryFileDto>();
        public List<ServiceRequirementDto> Requirements { get; set; } = new List<ServiceRequirementDto>();
        public List<ServiceStepDto> Steps { get; set; } = new List<ServiceStepDto>();
        public List<ServiceFaqDto> Faqs { get; set; } = new List<ServiceFaqDto>();
        public List<ServiceAttributeDto> Attributes { get; set; } = new List<ServiceAttributeDto>();
    }
}

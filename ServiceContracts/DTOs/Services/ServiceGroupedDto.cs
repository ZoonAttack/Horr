using System.Collections.Generic;

namespace ServiceContracts.DTOs.Services
{
    public class ServiceGroupedDto
    {
        public List<ServiceCatalogItemDto> Approved { get; set; } = new List<ServiceCatalogItemDto>();
        public List<ServiceCatalogItemDto> UnderReview { get; set; } = new List<ServiceCatalogItemDto>();
    }
}

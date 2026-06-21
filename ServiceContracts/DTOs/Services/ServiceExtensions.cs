using System.Linq;
using System.Collections.Generic;
using Entities.Marketplace;

namespace ServiceContracts.DTOs.Services
{
    public static class ServiceExtensions
    {
        public static ServiceCatalogItemDto ToDto(this ServiceCatalogItem service)
        {
            if (service == null) return null;

            return new ServiceCatalogItemDto
            {
                Id = service.Id,
                FreelancerId = service.FreelancerId,
                Title = service.Title,
                Description = service.Description,
                OriginalCurrency = service.Freelancer?.User?.PreferredCurrency ?? "USD",
                CoverImageUrl = service.CoverImageUrl,
                Price = service.Price,
                DeliveryTime = service.DeliveryTime,
                IsActive = service.IsActive,
                Status = service.Status.ToString(),
                CreatedAt = service.CreatedAt,
                UpdatedAt = service.UpdatedAt,
                Pricing = service.Pricing?.ToDto(),
                GalleryFiles = service.GalleryFiles?.Select(g => g.ToDto()).ToList() ?? new List<ServiceGalleryFileDto>(),
                Requirements = service.Requirements?.Select(r => r.ToDto()).ToList() ?? new List<ServiceRequirementDto>(),
                Steps = service.Steps?.Select(s => s.ToDto()).ToList() ?? new List<ServiceStepDto>(),
                Faqs = service.Faqs?.Select(f => f.ToDto()).ToList() ?? new List<ServiceFaqDto>(),
                Attributes = service.Attributes?.Select(a => a.ToDto()).ToList() ?? new List<ServiceAttributeDto>()
            };
        }

        public static ServicePricingDto ToDto(this ServicePricing pricing)
        {
            if (pricing == null) return null;

            return new ServicePricingDto
            {
                Id = pricing.Id,
                PriceFrom = pricing.PriceFrom,
                PriceTo = pricing.PriceTo,
                DeliveryDays = pricing.DeliveryDays,
                RevisionsIncluded = pricing.RevisionsIncluded
            };
        }

        public static ServiceGalleryFileDto ToDto(this ServiceGalleryFile file)
        {
            if (file == null) return null;

            return new ServiceGalleryFileDto
            {
                Id = file.Id,
                FileUrl = file.FileUrl,
                FileType = file.FileType,
                IsCover = file.IsCover,
                UploadedAt = file.UploadedAt
            };
        }

        public static ServiceRequirementDto ToDto(this ServiceRequirement req)
        {
            if (req == null) return null;

            return new ServiceRequirementDto
            {
                Id = req.Id,
                Question = req.Question,
                IsRequired = req.IsRequired
            };
        }

        public static ServiceStepDto ToDto(this ServiceStep step)
        {
            if (step == null) return null;

            return new ServiceStepDto
            {
                Id = step.Id,
                StepNumber = step.StepNumber,
                Title = step.Title,
                Description = step.Description
            };
        }

        public static ServiceFaqDto ToDto(this ServiceFaq faq)
        {
            if (faq == null) return null;

            return new ServiceFaqDto
            {
                Id = faq.Id,
                Question = faq.Question,
                Answer = faq.Answer
            };
        }

        public static ServiceAttributeDto ToDto(this ServiceAttribute attr)
        {
            if (attr == null) return null;

            return new ServiceAttributeDto
            {
                Id = attr.Id,
                Value = attr.Value
            };
        }

        /// <summary>
        /// Converts ServiceCatalogItem entity to ServiceReadDTO
        /// </summary>
        public static ServiceReadDTO Service_To_ServiceRead(this ServiceCatalogItem service)
        {
            if (service == null)
            {
                return null;
            }

            return new ServiceReadDTO
            {
                Id = service.Id,
                FreelancerId = service.FreelancerId,
                Title = service.Title,
                Description = service.Description,
                OriginalCurrency = service.Freelancer?.User?.PreferredCurrency ?? "USD",
                Price = service.Price,
                DeliveryTime = service.DeliveryTime,
                IsActive = service.IsActive,
                CreatedAt = service.CreatedAt,
                UpdatedAt = service.UpdatedAt
            };
        }

        /// <summary>
        /// Converts ServiceCreateDTO to ServiceCatalogItem entity
        /// </summary>
        public static ServiceCatalogItem ServiceCreate_To_Service(this ServiceCreateDTO createDto)
        {
            if (createDto == null)
            {
                return null;
            }

            return new ServiceCatalogItem
            {
                FreelancerId = createDto.FreelancerId,
                Title = createDto.Title,
                Description = createDto.Description,
                CoverImageUrl = createDto.CoverImageUrl,
                Price = createDto.Price,
                DeliveryTime = createDto.DeliveryTime,
                IsActive = true,
                Status = Entities.Enums.ServiceStatus.UnderReview,
                Pricing = createDto.Pricing != null ? new ServicePricing
                {
                    PriceFrom = createDto.Pricing.PriceFrom.GetValueOrDefault(),
                    PriceTo = createDto.Pricing.PriceTo,
                    DeliveryDays = createDto.Pricing.DeliveryDays.GetValueOrDefault(),
                    RevisionsIncluded = createDto.Pricing.RevisionsIncluded.GetValueOrDefault()
                } : null,
                GalleryFiles = createDto.GalleryFiles?.Select(g => new ServiceGalleryFile
                {
                    FileUrl = g.FileUrl,
                    FileType = g.FileType.GetValueOrDefault(),
                    IsCover = g.IsCover.GetValueOrDefault()
                }).ToList() ?? new List<ServiceGalleryFile>(),
                Requirements = createDto.Requirements?.Select(r => new ServiceRequirement
                {
                    Question = r.Question,
                    IsRequired = r.IsRequired.GetValueOrDefault()
                }).ToList() ?? new List<ServiceRequirement>(),
                Steps = createDto.Steps?.Select(s => new ServiceStep
                {
                    StepNumber = s.StepNumber.GetValueOrDefault(),
                    Title = s.Title,
                    Description = s.Description
                }).ToList() ?? new List<ServiceStep>(),
                Faqs = createDto.Faqs?.Select(f => new ServiceFaq
                {
                    Question = f.Question,
                    Answer = f.Answer
                }).ToList() ?? new List<ServiceFaq>(),
                Attributes = createDto.Attributes?.Select(a => new ServiceAttribute
                {
                    Value = a.Value
                }).ToList() ?? new List<ServiceAttribute>()
            };
        }

        /// <summary>
        /// Applies ServiceUpdateDTO to an existing ServiceCatalogItem entity
        /// </summary>
        public static void ServiceUpdate_To_Service(this ServiceCatalogItem service, ServiceUpdateDTO updateDto)
        {
            if (service == null || updateDto == null)
            {
                return;
            }

            if (!string.IsNullOrEmpty(updateDto.Title))
                service.Title = updateDto.Title;

            if (!string.IsNullOrEmpty(updateDto.Description))
                service.Description = updateDto.Description;

            if (updateDto.Price.HasValue)
                service.Price = updateDto.Price;

            if (!string.IsNullOrEmpty(updateDto.DeliveryTime))
                service.DeliveryTime = updateDto.DeliveryTime;

            service.IsActive = updateDto.IsActive;
        }
    }
}

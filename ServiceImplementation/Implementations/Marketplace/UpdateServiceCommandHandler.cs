using MediatR;
using Microsoft.EntityFrameworkCore;
using Entities;
using Entities.Marketplace;
using Entities.Enums;
using ServiceContracts.DTOs.Services;
using ServiceImplementation.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ServiceImplementation.Implementations.Marketplace
{
    public class UpdateServiceCommandHandler : IRequestHandler<UpdateServiceCommand, ServiceCatalogItemDto>
    {
        private readonly AppDbContext _context;

        public UpdateServiceCommandHandler(AppDbContext context)
        {
            _context = context;
        }

        public async Task<ServiceCatalogItemDto> Handle(UpdateServiceCommand request, CancellationToken cancellationToken)
        {
            var dto = request.Dto;

            var service = await _context.ServiceCatalogItems
                .Include(s => s.Pricing)
                .Include(s => s.GalleryFiles)
                .Include(s => s.Requirements)
                .Include(s => s.Steps)
                .Include(s => s.Faqs)
                .Include(s => s.Attributes)
                .FirstOrDefaultAsync(s => s.Id == dto.Id, cancellationToken);

            if (service == null)
            {
                throw new NotFoundException($"Service with ID {dto.Id} not found.");
            }

            if (service.FreelancerId != dto.FreelancerId)
            {
                throw new UnauthorizedAccessException("Unauthorized: You do not own this service.");
            }

            // Validation
            Validate(dto);

            // Update simple fields
            service.Title = dto.Title;
            service.Description = dto.Description;
            service.Price = dto.Price;
            service.DeliveryTime = dto.DeliveryTime;
            service.IsActive = dto.IsActive;
            service.CoverImageUrl = dto.CoverImageUrl;
            service.Status = ServiceStatus.UnderReview; // Any update triggers re-review

            // Replace Pricing
            if (service.Pricing != null)
            {
                _context.ServicePricings.Remove(service.Pricing);
            }
            if (dto.Pricing != null)
            {
                service.Pricing = new ServicePricing
                {
                    PriceFrom = dto.Pricing.PriceFrom,
                    PriceTo = dto.Pricing.PriceTo,
                    DeliveryDays = dto.Pricing.DeliveryDays,
                    RevisionsIncluded = dto.Pricing.RevisionsIncluded
                };
            }

            // Replace GalleryFiles
            _context.ServiceGalleryFiles.RemoveRange(service.GalleryFiles);
            service.GalleryFiles = dto.GalleryFiles?.Select(g => new ServiceGalleryFile
            {
                FileUrl = g.FileUrl,
                FileType = g.FileType,
                IsCover = g.IsCover,
                UploadedAt = DateTime.UtcNow
            }).ToList() ?? new List<ServiceGalleryFile>();

            // Replace Requirements
            _context.ServiceRequirements.RemoveRange(service.Requirements);
            service.Requirements = dto.Requirements?.Select(r => new ServiceRequirement
            {
                Question = r.Question,
                IsRequired = r.IsRequired
            }).ToList() ?? new List<ServiceRequirement>();

            // Replace Steps
            _context.ServiceSteps.RemoveRange(service.Steps);
            service.Steps = dto.Steps?.Select(s => new ServiceStep
            {
                StepNumber = s.StepNumber,
                Title = s.Title,
                Description = s.Description
            }).ToList() ?? new List<ServiceStep>();

            // Replace Faqs
            _context.ServiceFaqs.RemoveRange(service.Faqs);
            service.Faqs = dto.Faqs?.Select(f => new ServiceFaq
            {
                Question = f.Question,
                Answer = f.Answer
            }).ToList() ?? new List<ServiceFaq>();

            // Replace Attributes
            _context.ServiceAttributes.RemoveRange(service.Attributes);
            service.Attributes = dto.Attributes?.Select(a => new ServiceAttribute
            {
                Value = a.Value
            }).ToList() ?? new List<ServiceAttribute>();

            await _context.SaveChangesAsync(cancellationToken);

            return service.ToDto();
        }

        private void Validate(ServiceUpdateDTO dto)
        {
            var errors = new List<string>();

            if (string.IsNullOrWhiteSpace(dto.Title))
                errors.Add("Title: Title is required.");

            if (string.IsNullOrWhiteSpace(dto.Description) || dto.Description.Length < 120)
                errors.Add("Description: Description must be at least 120 characters.");

            if (dto.Attributes != null && dto.Attributes.Count > 3)
                errors.Add("Attributes: Maximum 3 attributes allowed.");

            if (dto.Faqs != null && dto.Faqs.Count > 5)
                errors.Add("Faqs: Maximum 5 FAQs allowed.");

            if (dto.Requirements == null || !dto.Requirements.Any())
                errors.Add("Requirements: At least 1 requirement is required.");

            if (dto.Steps == null || !dto.Steps.Any())
                errors.Add("Steps: At least 1 step is required.");

            if (errors.Any())
                throw new ValidationException("Validation failed", errors);
        }
    }
}

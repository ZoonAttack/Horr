using MediatR;
using Microsoft.EntityFrameworkCore;
using Entities;
using Entities.Marketplace;
using ServiceContracts.DTOs.Services;
using ServiceImplementation.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ServiceImplementation.Implementations.Marketplace
{
    public class CreateServiceCommandHandler : IRequestHandler<CreateServiceCommand, ServiceCatalogItemDto>
    {
        private readonly AppDbContext _context;

        public CreateServiceCommandHandler(AppDbContext context)
        {
            _context = context;
        }

        public async Task<ServiceCatalogItemDto> Handle(CreateServiceCommand request, CancellationToken cancellationToken)
        {
            var dto = request.Dto;

            // 1. Validation
            Validate(dto);

            // 2. Map DTO to Entity
            var service = dto.ServiceCreate_To_Service();

            // 3. Save
            _context.ServiceCatalogItems.Add(service);
            await _context.SaveChangesAsync(cancellationToken);

            // 4. Return DTO
            return service.ToDto();
        }

        private void Validate(ServiceCreateDTO dto)
        {
            var errors = new List<string>();

            // Title validation
            if (string.IsNullOrWhiteSpace(dto.Title))
            {
                errors.Add("Title: Title is required.");
            }

            // Description validation
            if (string.IsNullOrWhiteSpace(dto.Description) || dto.Description.Length < 120)
            {
                errors.Add("Description: Description must be at least 120 characters.");
            }

            // Attributes validation
            if (dto.Attributes != null && dto.Attributes.Count > 3)
            {
                errors.Add("Attributes: Maximum 3 attributes allowed.");
            }

            // FAQs validation
            if (dto.Faqs != null && dto.Faqs.Count > 5)
            {
                errors.Add("Faqs: Maximum 5 FAQs allowed.");
            }

            // Requirements validation
            if (dto.Requirements == null || !dto.Requirements.Any())
            {
                errors.Add("Requirements: At least 1 requirement is required.");
            }
            else
            {
                for (int i = 0; i < dto.Requirements.Count; i++)
                {
                    if (string.IsNullOrWhiteSpace(dto.Requirements[i].Question) || dto.Requirements[i].Question.Length < 10)
                    {
                        errors.Add($"Requirements[{i}].Question: Requirement must be at least 10 characters.");
                    }
                }
            }

            // Steps validation
            if (dto.Steps == null || !dto.Steps.Any())
            {
                errors.Add("Steps: At least 1 step is required.");
            }
            else
            {
                for (int i = 0; i < dto.Steps.Count; i++)
                {
                    if (string.IsNullOrWhiteSpace(dto.Steps[i].Title) || dto.Steps[i].Title.Length < 3)
                    {
                        errors.Add($"Steps[{i}].Title: Step title must be at least 3 characters.");
                    }
                }
            }

            if (errors.Any())
            {
                throw new ValidationException("Validation failed", errors);
            }
        }
    }
}

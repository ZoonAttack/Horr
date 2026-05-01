using MediatR;
using Microsoft.EntityFrameworkCore;
using Entities;
using Entities.Marketplace;
using Entities.Enums;
using ServiceContracts.DTOs.Services;
using ServiceImplementation.Exceptions;
using System;
using System.Collections.Generic;
using System.IO;
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

            // 1. Validation
            Validate(request, service);

            // 2. Update simple fields
            service.Title = dto.Title;
            service.Description = dto.Description;
            service.Price = dto.Price;
            service.DeliveryTime = dto.DeliveryTime;
            service.IsActive = dto.IsActive;
            service.CoverImageUrl = dto.CoverImageUrl;
            service.Status = ServiceStatus.UnderReview;

            // 3. Replace Collections
            // Pricing
            if (service.Pricing != null) _context.ServicePricings.Remove(service.Pricing);
            if (dto.Pricing != null)
            {
                service.Pricing = new ServicePricing
                {
                    PriceFrom = dto.Pricing.PriceFrom.GetValueOrDefault(),
                    PriceTo = dto.Pricing.PriceTo,
                    DeliveryDays = dto.Pricing.DeliveryDays.GetValueOrDefault(),
                    RevisionsIncluded = dto.Pricing.RevisionsIncluded.GetValueOrDefault()
                };
            }

            // Requirements
            _context.ServiceRequirements.RemoveRange(service.Requirements);
            service.Requirements = dto.Requirements?.Select(r => new ServiceRequirement
            {
                Question = r.Question,
                IsRequired = r.IsRequired.GetValueOrDefault()
            }).ToList() ?? new List<ServiceRequirement>();

            // Steps
            _context.ServiceSteps.RemoveRange(service.Steps);
            service.Steps = dto.Steps?.Select(s => new ServiceStep
            {
                StepNumber = s.StepNumber.GetValueOrDefault(),
                Title = s.Title,
                Description = s.Description
            }).ToList() ?? new List<ServiceStep>();

            // Faqs
            _context.ServiceFaqs.RemoveRange(service.Faqs);
            service.Faqs = dto.Faqs?.Select(f => new ServiceFaq
            {
                Question = f.Question,
                Answer = f.Answer
            }).ToList() ?? new List<ServiceFaq>();

            // Attributes
            _context.ServiceAttributes.RemoveRange(service.Attributes);
            service.Attributes = dto.Attributes?.Select(a => new ServiceAttribute
            {
                Value = a.Value
            }).ToList() ?? new List<ServiceAttribute>();

            // 4. Handle Gallery Files (Replacement or Append?)
            // Requirement says "replace all child collections (delete existing children, insert new ones)"
            // But if the user provides new files in the request, they should be added.
            // If they are NOT provided, maybe they want to keep existing ones?
            // "partial updates are forbidden" implies we should send the FULL state.
            // If GalleryFiles in DTO contains URLs, we should probably keep them.
            // If new files are in request, we add them.

            // Actually, to keep it simple and follow "replace all child collections", 
            // I'll clear existing and re-add from DTO + new uploads.
            
            _context.ServiceGalleryFiles.RemoveRange(service.GalleryFiles);
            service.GalleryFiles.Clear();

            // Re-add from DTO (existing files)
            if (dto.GalleryFiles != null)
            {
                foreach (var g in dto.GalleryFiles)
                {
                    service.GalleryFiles.Add(new ServiceGalleryFile
                    {
                        FileUrl = g.FileUrl,
                        FileType = g.FileType.GetValueOrDefault(),
                        IsCover = g.IsCover.GetValueOrDefault(),
                        UploadedAt = g.UploadedAt ?? DateTime.UtcNow
                    });
                }
            }

            // Handle new uploads
            var uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "services");
            if (!Directory.Exists(uploadPath)) Directory.CreateDirectory(uploadPath);

            if (request.Images != null)
            {
                foreach (var file in request.Images)
                {
                    var fileUrl = await SaveFileAsync(file, uploadPath, cancellationToken);
                    var isCover = request.CoverImageFileName == file.FileName;
                    if (isCover)
                    {
                        foreach (var f in service.GalleryFiles) f.IsCover = false;
                    }
                    service.GalleryFiles.Add(new ServiceGalleryFile
                    {
                        FileUrl = fileUrl,
                        FileType = ServiceGalleryFileType.Image,
                        IsCover = isCover,
                        UploadedAt = DateTime.UtcNow
                    });
                }
            }

            if (request.Video != null)
            {
                var fileUrl = await SaveFileAsync(request.Video, uploadPath, cancellationToken);
                service.GalleryFiles.Add(new ServiceGalleryFile
                {
                    FileUrl = fileUrl,
                    FileType = ServiceGalleryFileType.Video,
                    IsCover = false,
                    UploadedAt = DateTime.UtcNow
                });
            }

            if (request.Documents != null)
            {
                foreach (var file in request.Documents)
                {
                    var fileUrl = await SaveFileAsync(file, uploadPath, cancellationToken);
                    service.GalleryFiles.Add(new ServiceGalleryFile
                    {
                        FileUrl = fileUrl,
                        FileType = ServiceGalleryFileType.Document,
                        IsCover = false,
                        UploadedAt = DateTime.UtcNow
                    });
                }
            }

            await _context.SaveChangesAsync(cancellationToken);

            return service.ToDto();
        }

        private async Task<string> SaveFileAsync(Microsoft.AspNetCore.Http.IFormFile file, string uploadPath, CancellationToken cancellationToken)
        {
            var fileName = $"{Guid.NewGuid()}_{file.FileName}";
            var filePath = Path.Combine(uploadPath, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream, cancellationToken);
            }

            return $"/uploads/services/{fileName}";
        }

        private void Validate(UpdateServiceCommand command, ServiceCatalogItem service)
        {
            var dto = command.Dto;
            var errors = new List<string>();

            if (string.IsNullOrWhiteSpace(dto.Title)) errors.Add("Title: Title is required.");
            if (string.IsNullOrWhiteSpace(dto.Description) || dto.Description.Length < 120) errors.Add("Description: Description must be at least 120 characters.");
            
            // Files count check
            var totalImages = (dto.GalleryFiles?.Count(f => f.FileType == ServiceGalleryFileType.Image) ?? 0) + (command.Images?.Count ?? 0);
            if (totalImages > 15) errors.Add("Images: Maximum 15 images allowed.");

            var totalDocs = (dto.GalleryFiles?.Count(f => f.FileType == ServiceGalleryFileType.Document) ?? 0) + (command.Documents?.Count ?? 0);
            if (totalDocs > 3) errors.Add("Documents: Maximum 3 documents allowed.");

            // New files validation
            if (command.Images != null)
            {
                foreach (var file in command.Images)
                {
                    var ext = Path.GetExtension(file.FileName).ToLower();
                    if (ext != ".jpg" && ext != ".png" && ext != ".jpeg") errors.Add($"Images: File {file.FileName} is not a valid image.");
                    if (file.Length > 10 * 1024 * 1024) errors.Add($"Images: File {file.FileName} exceeds 10MB limit.");
                }
            }

            if (command.Video != null)
            {
                var ext = Path.GetExtension(command.Video.FileName).ToLower();
                if (ext != ".mp4") errors.Add("Video: Only .mp4 videos allowed.");
                if (command.Video.Length > 100 * 1024 * 1024) errors.Add("Video: Video exceeds 100MB limit.");
            }

            if (command.Documents != null)
            {
                foreach (var file in command.Documents)
                {
                    var ext = Path.GetExtension(file.FileName).ToLower();
                    if (ext != ".pdf") errors.Add($"Documents: File {file.FileName} is not a valid document.");
                    if (file.Length > 15 * 1024 * 1024) errors.Add($"Documents: File {file.FileName} exceeds 15MB limit.");
                }
            }

            if (errors.Any()) throw new ValidationException("Validation failed", errors);
        }
    }
}

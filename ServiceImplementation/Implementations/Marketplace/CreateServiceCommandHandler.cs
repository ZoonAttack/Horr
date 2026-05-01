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
            Validate(request);

            // 2. Map DTO to Entity
            var service = dto.ServiceCreate_To_Service();

            // 3. Handle File Uploads
            var uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "services");
            if (!Directory.Exists(uploadPath))
            {
                Directory.CreateDirectory(uploadPath);
            }

            // Images
            if (request.Images != null)
            {
                foreach (var file in request.Images)
                {
                    var fileUrl = await SaveFileAsync(file, uploadPath, cancellationToken);
                    var isCover = request.CoverImageFileName == file.FileName;
                    
                    service.GalleryFiles.Add(new ServiceGalleryFile
                    {
                        FileUrl = fileUrl,
                        FileType = ServiceGalleryFileType.Image,
                        IsCover = isCover,
                        UploadedAt = DateTime.UtcNow
                    });
                }
            }

            // Video
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

            // Documents
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

            // 4. Save
            _context.ServiceCatalogItems.Add(service);
            await _context.SaveChangesAsync(cancellationToken);

            // 5. Return DTO
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

        private void Validate(CreateServiceCommand command)
        {
            var dto = command.Dto;
            var errors = new List<string>();

            // Basic fields
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

            // Files
            if (command.Images != null)
            {
                if (command.Images.Count > 15)
                    errors.Add("Images: Maximum 15 images allowed.");

                foreach (var file in command.Images)
                {
                    var ext = Path.GetExtension(file.FileName).ToLower();
                    if (ext != ".jpg" && ext != ".png" && ext != ".jpeg")
                        errors.Add($"Images: File {file.FileName} is not a valid image. Only .jpg and .png allowed.");

                    if (file.Length > 10 * 1024 * 1024)
                        errors.Add($"Images: File {file.FileName} exceeds 10MB limit.");
                }
            }

            if (command.Video != null)
            {
                var ext = Path.GetExtension(command.Video.FileName).ToLower();
                if (ext != ".mp4")
                    errors.Add("Video: Only .mp4 videos allowed.");

                if (command.Video.Length > 100 * 1024 * 1024)
                    errors.Add("Video: Video exceeds 100MB limit.");
            }

            if (command.Documents != null)
            {
                if (command.Documents.Count > 3)
                    errors.Add("Documents: Maximum 3 documents allowed.");

                foreach (var file in command.Documents)
                {
                    var ext = Path.GetExtension(file.FileName).ToLower();
                    if (ext != ".pdf")
                        errors.Add($"Documents: File {file.FileName} is not a valid document. Only .pdf allowed.");

                    if (file.Length > 15 * 1024 * 1024)
                        errors.Add($"Documents: File {file.FileName} exceeds 15MB limit.");
                }
            }

            if (errors.Any())
            {
                throw new ValidationException("Validation failed", errors);
            }
        }
    }
}

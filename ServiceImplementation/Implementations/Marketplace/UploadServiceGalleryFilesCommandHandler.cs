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
    public class UploadServiceGalleryFilesCommandHandler : IRequestHandler<UploadServiceGalleryFilesCommand, ServiceCatalogItemDto>
    {
        private readonly AppDbContext _context;

        public UploadServiceGalleryFilesCommandHandler(AppDbContext context)
        {
            _context = context;
        }

        public async Task<ServiceCatalogItemDto> Handle(UploadServiceGalleryFilesCommand request, CancellationToken cancellationToken)
        {
            var service = await _context.ServiceCatalogItems
                .Include(s => s.GalleryFiles)
                .Include(s => s.Pricing)
                .Include(s => s.Requirements)
                .Include(s => s.Steps)
                .Include(s => s.Faqs)
                .Include(s => s.Attributes)
                .FirstOrDefaultAsync(s => s.Id == request.ServiceId, cancellationToken);

            if (service == null)
            {
                throw new NotFoundException($"Service with ID {request.ServiceId} not found.");
            }

            if (service.FreelancerId != request.FreelancerId)
            {
                throw new UnauthorizedAccessException("Unauthorized: You do not own this service.");
            }

            // Validation
            Validate(request, service);

            var uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "services");
            if (!Directory.Exists(uploadPath))
            {
                Directory.CreateDirectory(uploadPath);
            }

            // Handle Images
            if (request.Images != null)
            {
                foreach (var file in request.Images)
                {
                    var fileUrl = await SaveFileAsync(file, uploadPath, cancellationToken);
                    var isCover = request.CoverImageFileName == file.FileName;
                    
                    if (isCover)
                    {
                        // Reset all existing cover flags
                        foreach (var existing in service.GalleryFiles)
                        {
                            existing.IsCover = false;
                        }
                    }

                    var galleryFile = new ServiceGalleryFile
                    {
                        ServiceId = service.Id,
                        FileUrl = fileUrl,
                        FileType = ServiceGalleryFileType.Image,
                        IsCover = isCover,
                        UploadedAt = DateTime.UtcNow
                    };
                    service.GalleryFiles.Add(galleryFile);
                }
            }

            // Handle Video
            if (request.Video != null)
            {
                var fileUrl = await SaveFileAsync(request.Video, uploadPath, cancellationToken);
                var galleryFile = new ServiceGalleryFile
                {
                    ServiceId = service.Id,
                    FileUrl = fileUrl,
                    FileType = ServiceGalleryFileType.Video,
                    IsCover = false,
                    UploadedAt = DateTime.UtcNow
                };
                service.GalleryFiles.Add(galleryFile);
            }

            // Handle Documents
            if (request.Documents != null)
            {
                foreach (var file in request.Documents)
                {
                    var fileUrl = await SaveFileAsync(file, uploadPath, cancellationToken);
                    var galleryFile = new ServiceGalleryFile
                    {
                        ServiceId = service.Id,
                        FileUrl = fileUrl,
                        FileType = ServiceGalleryFileType.Document,
                        IsCover = false,
                        UploadedAt = DateTime.UtcNow
                    };
                    service.GalleryFiles.Add(galleryFile);
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

        private void Validate(UploadServiceGalleryFilesCommand request, ServiceCatalogItem service)
        {
            var errors = new List<string>();

            // 1. Images Validation
            if (request.Images != null && request.Images.Any())
            {
                var existingImageCount = service.GalleryFiles.Count(f => f.FileType == ServiceGalleryFileType.Image);
                if (existingImageCount + request.Images.Count > 15)
                {
                    errors.Add("Images: Maximum 15 images allowed.");
                }

                foreach (var file in request.Images)
                {
                    var ext = Path.GetExtension(file.FileName).ToLower();
                    if (ext != ".jpg" && ext != ".png" && ext != ".jpeg")
                    {
                        errors.Add($"Images: File {file.FileName} is not a valid image. Only .jpg and .png allowed.");
                    }

                    if (file.Length > 10 * 1024 * 1024)
                    {
                        errors.Add($"Images: File {file.FileName} exceeds 10MB limit.");
                    }
                }
            }

            // 2. Video Validation
            if (request.Video != null)
            {
                var ext = Path.GetExtension(request.Video.FileName).ToLower();
                if (ext != ".mp4")
                {
                    errors.Add("Video: Only .mp4 videos allowed.");
                }

                if (request.Video.Length > 100 * 1024 * 1024)
                {
                    errors.Add("Video: Video exceeds 100MB limit.");
                }
            }

            // 3. Documents Validation
            if (request.Documents != null && request.Documents.Any())
            {
                var existingDocCount = service.GalleryFiles.Count(f => f.FileType == ServiceGalleryFileType.Document);
                if (existingDocCount + request.Documents.Count > 3)
                {
                    errors.Add("Documents: Maximum 3 documents allowed.");
                }

                foreach (var file in request.Documents)
                {
                    var ext = Path.GetExtension(file.FileName).ToLower();
                    if (ext != ".pdf")
                    {
                        errors.Add($"Documents: File {file.FileName} is not a valid document. Only .pdf allowed.");
                    }

                    if (file.Length > 15 * 1024 * 1024)
                    {
                        errors.Add($"Documents: File {file.FileName} exceeds 15MB limit.");
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

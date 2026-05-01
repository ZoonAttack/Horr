using MediatR;
using Microsoft.AspNetCore.Http;
using ServiceContracts.DTOs.Services;
using System.Collections.Generic;

namespace ServiceImplementation.Implementations.Marketplace
{
    public record UploadServiceGalleryFilesCommand(
        string ServiceId,
        string FreelancerId,
        List<IFormFile>? Images = null,
        IFormFile? Video = null,
        List<IFormFile>? Documents = null,
        string? CoverImageFileName = null) : IRequest<ServiceCatalogItemDto>;
}

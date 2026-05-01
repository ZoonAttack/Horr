using MediatR;
using ServiceContracts.DTOs.Services;

namespace ServiceImplementation.Implementations.Marketplace
{
    public record CreateServiceCommand(
        ServiceCreateDTO Dto, 
        List<Microsoft.AspNetCore.Http.IFormFile>? Images = null,
        Microsoft.AspNetCore.Http.IFormFile? Video = null,
        List<Microsoft.AspNetCore.Http.IFormFile>? Documents = null,
        string? CoverImageFileName = null) : IRequest<ServiceCatalogItemDto>;
}

using MediatR;
using ServiceContracts.DTOs.Services;

namespace ServiceImplementation.Implementations.Marketplace
{
    public record UpdateServiceCommand(ServiceUpdateDTO Dto) : IRequest<ServiceCatalogItemDto>;
}

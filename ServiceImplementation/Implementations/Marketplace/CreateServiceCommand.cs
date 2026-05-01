using MediatR;
using ServiceContracts.DTOs.Services;

namespace ServiceImplementation.Implementations.Marketplace
{
    public record CreateServiceCommand(ServiceCreateDTO Dto) : IRequest<ServiceCatalogItemDto>;
}

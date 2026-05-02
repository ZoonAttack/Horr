using MediatR;
using ServiceContracts.DTOs.Services;

namespace ServiceImplementation.Implementations.Marketplace
{
    public record GetServiceByIdQuery(string Id, string FreelancerId) : IRequest<ServiceCatalogItemDto>;
}

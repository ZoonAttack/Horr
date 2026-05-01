using MediatR;
using ServiceContracts.DTOs.Services;

namespace ServiceImplementation.Implementations.Marketplace
{
    public record GetMyServicesQuery(string FreelancerId) : IRequest<ServiceGroupedDto>;
}

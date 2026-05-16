using MediatR;
using ServiceContracts.DTOs.Responses;

namespace ServiceImplementation.Implementations.ClientImplementation.DiscoveryCQRS
{
    public record SaveFreelancerCommand(string ClientId, string FreelancerId) : IRequest<Result<bool>>;
    public record UnsaveFreelancerCommand(string ClientId, string FreelancerId) : IRequest<Result<bool>>;
}

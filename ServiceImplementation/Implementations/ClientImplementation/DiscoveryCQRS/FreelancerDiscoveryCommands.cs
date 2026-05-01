using MediatR;

namespace ServiceImplementation.Implementations.ClientImplementation.DiscoveryCQRS
{
    public record SaveFreelancerCommand(string ClientId, string FreelancerId) : IRequest<bool>;
    public record UnsaveFreelancerCommand(string ClientId, string FreelancerId) : IRequest<bool>;
}

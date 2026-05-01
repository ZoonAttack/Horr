using MediatR;

namespace ServiceImplementation.Implementations.Marketplace
{
    public record DeleteServiceCommand(string Id, string FreelancerId) : IRequest;
}

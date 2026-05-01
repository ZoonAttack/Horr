using MediatR;

namespace ServiceImplementation.Implementations.Contracts
{
    public record DeclineOfferCommand(int ContractId, string FreelancerId) : IRequest<bool>;
}

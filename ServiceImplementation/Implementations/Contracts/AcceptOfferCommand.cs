using MediatR;

namespace ServiceImplementation.Implementations.Contracts
{
    public record AcceptOfferCommand(int ContractId, string FreelancerId) : IRequest<bool>;
}

using MediatR;
using ServiceContracts.DTOs.Responses;

namespace ServiceImplementation.Implementations.Contracts
{
    public record DeclineOfferCommand(int ContractId, string FreelancerId) : IRequest<Result<bool>>;
}

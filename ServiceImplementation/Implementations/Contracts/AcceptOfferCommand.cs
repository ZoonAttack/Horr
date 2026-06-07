using MediatR;
using ServiceContracts.DTOs.Responses;

namespace ServiceImplementation.Implementations.Contracts
{
    public record AcceptOfferCommand(int ContractId, string FreelancerId) : IRequest<Result<bool>>;
}

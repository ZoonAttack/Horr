using MediatR;
using ServiceContracts.DTOs.Contract;

namespace ServiceImplementation.Implementations.Proposals
{
    public record AcceptOfferCommand(int ProposalId, string FreelancerId) : IRequest<ContractReadDTO>;
}

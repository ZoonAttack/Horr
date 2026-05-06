using MediatR;
using ServiceContracts.DTOs.Responses;

namespace ServiceImplementation.Implementations.Proposals
{
    public record WithdrawProposalCommand(int ProposalId, string FreelancerId) : IRequest<Result<bool>>;
}

using MediatR;
using ServiceContracts.DTOs.Proposal;
using ServiceContracts.DTOs.Responses;

namespace ServiceImplementation.Implementations.Proposals
{
    public record UpdateProposalCommand(int ProposalId, ProposalUpdateDTO Dto, string FreelancerId) : IRequest<Result<ProposalReadDTO>>;
}

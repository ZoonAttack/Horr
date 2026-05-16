using MediatR;
using ServiceContracts.DTOs.Proposal;
using ServiceContracts.DTOs.Responses;

namespace ServiceImplementation.Implementations.Proposals
{
    public record CreateProposalCommand(ProposalCreateDTO Dto, string FreelancerId) : IRequest<Result<ProposalReadDTO>>;
}

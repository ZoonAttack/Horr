using MediatR;
using ServiceContracts.DTOs.Proposal;
using Entities.Enums;

namespace ServiceImplementation.Implementations.Proposals
{
    public record CreateProposalCommand(ProposalCreateDTO Dto, string FreelancerId) : IRequest<ProposalReadDTO>;
}

using MediatR;

namespace ServiceImplementation.Implementations.Proposals
{
    public record WithdrawProposalCommand(int ProposalId, string FreelancerId) : IRequest;
}

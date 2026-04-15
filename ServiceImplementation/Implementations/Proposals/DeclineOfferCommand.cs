using MediatR;

namespace ServiceImplementation.Implementations.Proposals
{
    public record DeclineOfferCommand(int ProposalId, string FreelancerId) : IRequest<Unit>;
}

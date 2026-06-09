using MediatR;
using ServiceContracts.DTOs.Responses;

namespace ServiceImplementation.Implementations.Proposals
{
    public record RejectProposalCommand(int ProposalId, string ClientId) : IRequest<Result<bool>>;
}

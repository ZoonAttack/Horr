using MediatR;
using ServiceContracts.DTOs.Proposal;

namespace ServiceImplementation.Implementations.Proposals
{
    public record GetMyProposalsQuery(string FreelancerId) : IRequest<MyProposalsResponseDto>;
}

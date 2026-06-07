using MediatR;
using ServiceContracts.DTOs.Proposal;
using ServiceContracts.DTOs.Responses;

namespace ServiceImplementation.Implementations.Proposals
{
    public record GetMyProposalsQuery(string FreelancerId) : IRequest<Result<MyProposalsResponseDto>>;
}

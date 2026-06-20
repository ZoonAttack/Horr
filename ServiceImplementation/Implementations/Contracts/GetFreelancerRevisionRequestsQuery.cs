using System.Collections.Generic;
using MediatR;
using ServiceContracts.DTOs.Contract;
using ServiceContracts.DTOs.Responses;

namespace ServiceImplementation.Implementations.Contracts
{
    public record GetFreelancerRevisionRequestsQuery(
        string FreelancerId,
        int? ContractId
    ) : IRequest<Result<List<RevisionRequestDto>>>;
}

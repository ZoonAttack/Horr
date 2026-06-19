using System.Collections.Generic;
using MediatR;
using ServiceContracts.DTOs.Contract;
using ServiceContracts.DTOs.Responses;

namespace ServiceImplementation.Implementations.Contracts
{
    public record GetPendingAdditionalRevisionsQuery(
        string FreelancerId
    ) : IRequest<Result<IEnumerable<AdditionalRevisionRequestDto>>>;
}
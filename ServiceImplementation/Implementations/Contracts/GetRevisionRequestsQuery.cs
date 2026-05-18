using System.Collections.Generic;
using MediatR;
using ServiceContracts.DTOs.Contract;

namespace ServiceImplementation.Implementations.Contracts
{
    public record GetRevisionRequestsQuery() : IRequest<List<RevisionRequestDto>>;
}

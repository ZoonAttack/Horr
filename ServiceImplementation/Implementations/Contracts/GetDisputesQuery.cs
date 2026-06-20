using MediatR;
using ServiceContracts.DTOs.Contract;
using ServiceContracts.DTOs.Responses;
using Services;

namespace ServiceImplementation.Implementations.Contracts
{
    public record GetDisputesQuery(int Page = 1, int PageSize = 10)
        : IRequest<Result<PagedResult<DisputeAdminDto>>>;
}

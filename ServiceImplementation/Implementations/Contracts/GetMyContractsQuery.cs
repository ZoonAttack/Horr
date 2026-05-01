using MediatR;
using Entities.Enums;
using ServiceContracts.DTOs.Contract;
using Services;

namespace ServiceImplementation.Implementations.Contracts
{
    public record GetMyContractsQuery(string UserId, string UserRole, ContractStatus? Status = null, int Page = 1, int PageSize = 10) : IRequest<Services.PagedResult<ContractReadDTO>>;
}

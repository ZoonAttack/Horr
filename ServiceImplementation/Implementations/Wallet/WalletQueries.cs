using MediatR;
using Services;
using ServiceContracts.DTOs.Wallet;
using ServiceContracts.DTOs.Responses;

namespace ServiceImplementation.Implementations.Wallet
{
    public record GetMyDepositRequestsQuery(string UserId, int Page = 1, int PageSize = 10) : IRequest<Result<PagedResult<DepositRequestDto>>>;
    public record GetMyWithdrawalRequestsQuery(string UserId, int Page = 1, int PageSize = 10) : IRequest<Result<PagedResult<WithdrawalRequestDto>>>;
    public record GetPendingDepositRequestsQuery() : IRequest<Result<IEnumerable<DepositRequestDto>>>;
    public record GetPendingWithdrawalRequestsQuery() : IRequest<Result<IEnumerable<WithdrawalRequestDto>>>;
    public record GetWalletBalanceQuery(string UserId) : IRequest<Result<WalletBalanceDto>>;
}

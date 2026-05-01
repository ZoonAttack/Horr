using MediatR;
using Services;
using ServiceContracts.DTOs.Wallet;

namespace ServiceImplementation.Implementations.Wallet
{
    public record GetMyDepositRequestsQuery(string UserId, int Page = 1, int PageSize = 10) : IRequest<PagedResult<DepositRequestDto>>;
    public record GetMyWithdrawalRequestsQuery(string UserId, int Page = 1, int PageSize = 10) : IRequest<PagedResult<WithdrawalRequestDto>>;
    public record GetPendingDepositRequestsQuery() : IRequest<IEnumerable<DepositRequestDto>>;
    public record GetPendingWithdrawalRequestsQuery() : IRequest<IEnumerable<WithdrawalRequestDto>>;
    public record GetWalletBalanceQuery(string UserId) : IRequest<WalletBalanceDto>;
}

using MediatR;
using ServiceContracts.DTOs.Wallet;
using Entities.Enums;

namespace ServiceImplementation.Implementations.Wallet
{
    public record SubmitWithdrawalRequestCommand(
        string? FreelancerId,
        decimal Amount,
        WithdrawalMethod Method,
        string? InstapayUsername,
        string? BankAccountDetails,
        string? EWalletNumber
    ) : IRequest<WithdrawalRequestDto>;
}

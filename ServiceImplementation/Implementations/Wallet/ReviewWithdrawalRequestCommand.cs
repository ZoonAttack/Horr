using MediatR;
using ServiceContracts.DTOs.Wallet;
using Entities.Enums;

namespace ServiceImplementation.Implementations.Wallet
{
    public record ReviewWithdrawalRequestCommand(
        Guid RequestId,
        WithdrawalStatus Status,
        string? AdminNote
    ) : IRequest<WithdrawalRequestDto>;
}

using MediatR;
using Entities.Enums;
using ServiceContracts.DTOs.Wallet;
using ServiceContracts.DTOs.Responses;

namespace ServiceImplementation.Implementations.Wallet
{
    public record ReviewWithdrawalRequestCommand(string RequestId, WithdrawalStatus Status, string? AdminNote) : IRequest<Result<WithdrawalRequestDto>>;
}

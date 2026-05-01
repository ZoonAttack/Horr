using MediatR;
using ServiceContracts.DTOs.Wallet;
using Entities.Enums;

namespace ServiceImplementation.Implementations.Wallet
{
    public record ReviewDepositRequestCommand(
        string RequestId,
        DepositStatus Status,
        string? AdminNote
    ) : IRequest<DepositRequestDto>;
}

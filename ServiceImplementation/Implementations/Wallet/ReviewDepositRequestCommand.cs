using MediatR;
using ServiceContracts.DTOs.Wallet;
using Entities.Enums;

namespace ServiceImplementation.Implementations.Wallet
{
    public record ReviewDepositRequestCommand(
        Guid RequestId,
        DepositStatus Status,
        string? AdminNote
    ) : IRequest<DepositRequestDto>;
}

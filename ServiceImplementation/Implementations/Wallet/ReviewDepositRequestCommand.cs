using MediatR;
using Entities.Enums;
using ServiceContracts.DTOs.Wallet;
using ServiceContracts.DTOs.Responses;

namespace ServiceImplementation.Implementations.Wallet
{
    public record ReviewDepositRequestCommand(string RequestId, DepositStatus Status, string? AdminNote) : IRequest<Result<DepositRequestDto>>;
}

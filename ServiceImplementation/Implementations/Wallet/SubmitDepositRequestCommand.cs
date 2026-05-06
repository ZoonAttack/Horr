using MediatR;
using ServiceContracts.DTOs.Wallet;
using ServiceContracts.DTOs.Responses;
using Microsoft.AspNetCore.Http;

namespace ServiceImplementation.Implementations.Wallet
{
    public record SubmitDepositRequestCommand(
        string? ClientId,
        decimal Amount,
        string ReceiptNumber,
        IFormFile? ReceiptPhoto
    ) : IRequest<Result<DepositRequestDto>>;
}

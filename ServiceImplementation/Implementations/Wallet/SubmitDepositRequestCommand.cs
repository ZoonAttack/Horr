using MediatR;
using ServiceContracts.DTOs.Wallet;
using Microsoft.AspNetCore.Http;

namespace ServiceImplementation.Implementations.Wallet
{
    public record SubmitDepositRequestCommand(
        string ClientId,
        decimal Amount,
        string ReceiptNumber,
        IFormFile? ReceiptPhoto
    ) : IRequest<DepositRequestDto>;
}

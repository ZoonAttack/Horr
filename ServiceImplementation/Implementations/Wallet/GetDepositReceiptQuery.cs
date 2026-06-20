using MediatR;
using ServiceContracts.DTOs.Contract;
using ServiceContracts.DTOs.Responses;
using System;

namespace ServiceImplementation.Implementations.Wallet
{
    public record GetDepositReceiptQuery(
        Guid Id,
        string RequestingUserId,
        bool IsAdmin
    ) : IRequest<Result<DownloadFileResult>>;
}

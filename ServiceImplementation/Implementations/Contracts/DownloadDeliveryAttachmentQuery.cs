using System;
using MediatR;
using ServiceContracts.DTOs.Contract;
using ServiceContracts.DTOs.Responses;

namespace ServiceImplementation.Implementations.Contracts
{
    public record DownloadDeliveryAttachmentQuery(
        Guid AttachmentId,
        string RequestingUserId,
        bool IsAdmin = false,
        bool IsSpecialist = false
    ) : IRequest<Result<DownloadFileResult>>;
}

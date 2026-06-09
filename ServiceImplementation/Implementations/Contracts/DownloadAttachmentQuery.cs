using MediatR;
using ServiceContracts.DTOs.Contract;
using ServiceContracts.DTOs.Responses;

namespace ServiceImplementation.Implementations.Contracts
{
    public record DownloadAttachmentQuery(
        int ContractId,
        int DeliveryId,
        int AttachmentId,
        string RequestingUserId
    ) : IRequest<Result<DownloadFileResult>>;
}

using MediatR;
using Microsoft.AspNetCore.Http;
using ServiceContracts.DTOs.Chat;
using ServiceContracts.DTOs.Responses;

namespace ServiceImplementation.Implementations.Communication
{
    public record UploadChatFileCommand(
        string ChatId,
        string SenderId,
        IFormFile File
    ) : IRequest<Result<MessageDto>>;
}

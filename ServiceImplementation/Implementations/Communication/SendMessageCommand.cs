using MediatR;
using Microsoft.AspNetCore.Http;
using ServiceContracts.DTOs.Chat;
using ServiceContracts.DTOs.Responses;
using System.Collections.Generic;

namespace ServiceImplementation.Implementations.Communication
{
    public record SendMessageCommand(
        string ConversationId,
        string SenderId,
        string Body,
        List<IFormFile>? Files = null,
        string? JobPostId = null,
        string? ReceiverId = null
    ) : IRequest<Result<MessageDto>>;
}

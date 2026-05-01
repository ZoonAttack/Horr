using MediatR;
using Microsoft.AspNetCore.Http;
using ServiceContracts.DTOs.Chat;

namespace ServiceImplementation.Implementations.Communication
{
    public record SendMessageCommand(
        string ConversationId,
        string SenderId,
        string Body,
        List<IFormFile>? Files = null
    ) : IRequest<MessageDto>;
}

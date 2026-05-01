using MediatR;
using ServiceContracts.DTOs.Chat;
using Services;

namespace ServiceImplementation.Implementations.Communication
{
    public record GetMessagesQuery(
        string ConversationId,
        string UserId,
        int PageNumber = 1,
        int PageSize = 20
    ) : IRequest<PagedResult<MessageDto>>;
}

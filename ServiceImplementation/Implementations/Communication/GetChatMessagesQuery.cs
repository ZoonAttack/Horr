using MediatR;
using ServiceContracts.DTOs.Chat;
using ServiceContracts.DTOs.Responses;
using Services;

namespace ServiceImplementation.Implementations.Communication
{
    public record GetChatMessagesQuery(
        string ChatId,
        string UserId,
        int Page = 1,
        int PageSize = 30
    ) : IRequest<Result<PagedResult<MessageDto>>>;
}

using MediatR;
using ServiceContracts.DTOs.Chat;
using Services;
using ServiceContracts.DTOs.Responses;

namespace ServiceImplementation.Implementations.Communication
{
    public record GetMessagesQuery(
        string ChatId,
        string UserId,
        int PageNumber = 1,
        int PageSize = 20
    ) : IRequest<Result<PagedResult<MessageDto>>>;
}

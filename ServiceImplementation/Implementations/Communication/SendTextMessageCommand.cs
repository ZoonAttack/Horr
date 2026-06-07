using MediatR;
using ServiceContracts.DTOs.Chat;
using ServiceContracts.DTOs.Responses;

namespace ServiceImplementation.Implementations.Communication
{
    public record SendTextMessageCommand(
        string ChatId,
        string SenderId,
        string Text
    ) : IRequest<Result<MessageDto>>;
}

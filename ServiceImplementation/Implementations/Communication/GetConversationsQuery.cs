using MediatR;
using ServiceContracts.DTOs.Chat;
using ServiceContracts.DTOs.Responses;
using System.Collections.Generic;

namespace ServiceImplementation.Implementations.Communication
{
    public record GetConversationsQuery(string UserId) : IRequest<Result<List<ConversationDto>>>;
}

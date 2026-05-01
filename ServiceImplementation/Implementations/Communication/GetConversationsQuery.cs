using MediatR;
using ServiceContracts.DTOs.Chat;
using System.Collections.Generic;

namespace ServiceImplementation.Implementations.Communication
{
    public record GetConversationsQuery(string UserId) : IRequest<List<ConversationDto>>;
}

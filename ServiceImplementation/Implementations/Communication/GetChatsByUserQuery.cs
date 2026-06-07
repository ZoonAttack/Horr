using MediatR;
using Entities.Enums;
using ServiceContracts.DTOs.Chat;
using ServiceContracts.DTOs.Responses;
using System.Collections.Generic;

namespace ServiceImplementation.Implementations.Communication
{
    public record GetChatsByUserQuery(string UserId, UserRole Role) : IRequest<Result<List<ChatSummaryDto>>>;
}

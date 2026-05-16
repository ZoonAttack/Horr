using Entities;
using Entities.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ServiceContracts.DTOs.Chat;
using ServiceImplementation.Mappings.Communication;
using ServiceContracts.DTOs.Responses;
using ServiceImplementation.Helpers;

namespace ServiceImplementation.Implementations.Communication
{
    public class GetConversationsQueryHandler : IRequestHandler<GetConversationsQuery, Result<List<ConversationDto>>>
    {
        private readonly AppDbContext _context;

        public GetConversationsQueryHandler(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Result<List<ConversationDto>>> Handle(GetConversationsQuery request, CancellationToken cancellationToken)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == request.UserId, cancellationToken);
            if (user == null || user.IsDeleted)
            {
                return new Result<List<ConversationDto>>
                {
                    Succeeded = false,
                    ErrorCode = ErrorCodes.AccountDeleted,
                    Message = "Account not found or is deleted."
                };
            }

            var conversations = await _context.ConversationParticipants
                .Where(p => p.UserId == request.UserId)
                .Select(p => p.Conversation)
                .Select(c => new
                {
                    c.Id,
                    c.CreatedAt,
                    LastMessage = c.Messages
                        .OrderByDescending(m => m.SentAt)
                        .FirstOrDefault(),
                    UnreadCount = c.Messages
                        .Count(m => m.Status == MessageStatus.Unread && m.SenderId != request.UserId)
                })
                .ToListAsync(cancellationToken);

            var result = conversations.Select(c => new ConversationDto
            {
                Id = c.Id,
                CreatedAt = c.CreatedAt,
                LastMessagePreview = MessagePreviewHelper.GetPreview(c.LastMessage?.Body),
                UnreadCount = c.UnreadCount
            }).ToList();

            return new Result<List<ConversationDto>> { Succeeded = true, Data = result };
        }
    }
}

using Entities;
using Entities.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ServiceContracts.DTOs.Chat;
using ServiceImplementation.Mappings.Communication;

namespace ServiceImplementation.Implementations.Communication
{
    public class GetConversationsQueryHandler : IRequestHandler<GetConversationsQuery, List<ConversationDto>>
    {
        private readonly AppDbContext _context;

        public GetConversationsQueryHandler(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<ConversationDto>> Handle(GetConversationsQuery request, CancellationToken cancellationToken)
        {
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

            return conversations.Select(c => new ConversationDto
            {
                Id = c.Id,
                CreatedAt = c.CreatedAt,
                LastMessagePreview = MessagePreviewHelper.GetPreview(c.LastMessage?.Body),
                UnreadCount = c.UnreadCount
            }).ToList();
        }
    }
}

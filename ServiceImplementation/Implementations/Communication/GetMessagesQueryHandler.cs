using Entities;
using Entities.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ServiceContracts.DTOs.Chat;
using ServiceImplementation.Exceptions;
using Services;

namespace ServiceImplementation.Implementations.Communication
{
    public class GetMessagesQueryHandler : IRequestHandler<GetMessagesQuery, PagedResult<MessageDto>>
    {
        private readonly AppDbContext _context;

        public GetMessagesQueryHandler(AppDbContext context)
        {
            _context = context;
        }

        public async Task<PagedResult<MessageDto>> Handle(GetMessagesQuery request, CancellationToken cancellationToken)
        {
            // Verify conversation and participation
            var isParticipant = await _context.ConversationParticipants
                .AnyAsync(p => p.ConversationId == request.ConversationId && p.UserId == request.UserId, cancellationToken);

            if (!isParticipant)
            {
                throw new NotFoundException($"Conversation with ID {request.ConversationId} not found or you are not a participant.");
            }

            using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);

            try
            {
                // Mark unread messages from other users as read
                var unreadMessages = await _context.Messages
                    .Where(m => m.ConversationId == request.ConversationId && 
                                m.SenderId != request.UserId && 
                                m.Status == MessageStatus.Unread)
                    .ToListAsync(cancellationToken);

                if (unreadMessages.Any())
                {
                    foreach (var msg in unreadMessages)
                    {
                        msg.Status = MessageStatus.Read;
                    }
                    await _context.SaveChangesAsync(cancellationToken);
                }

                // Fetch paginated messages
                var query = _context.Messages
                    .Where(m => m.ConversationId == request.ConversationId)
                    .OrderByDescending(m => m.SentAt);

                var totalCount = await query.CountAsync(cancellationToken);
                var items = await query
                    .Skip((request.PageNumber - 1) * request.PageSize)
                    .Take(request.PageSize)
                    .Select(m => new MessageDto
                    {
                        Id = m.Id,
                        ConversationId = m.ConversationId,
                        SenderId = m.SenderId,
                        Body = m.Body,
                        Status = m.Status,
                        SentAt = m.SentAt
                    })
                    .ToListAsync(cancellationToken);

                await transaction.CommitAsync(cancellationToken);

                return new PagedResult<MessageDto>
                {
                    Items = items,
                    TotalCount = totalCount,
                    Page = request.PageNumber,
                    PageSize = request.PageSize
                };
            }
            catch (Exception)
            {
                await transaction.RollbackAsync(cancellationToken);
                throw;
            }
        }
    }
}

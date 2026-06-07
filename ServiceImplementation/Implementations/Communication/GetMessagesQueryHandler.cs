using Entities;
using Entities.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ServiceContracts.DTOs.Chat;
using ServiceImplementation.Exceptions;
using Services;
using ServiceContracts.DTOs.Responses;
using ServiceImplementation.Helpers;

namespace ServiceImplementation.Implementations.Communication
{
    public class GetMessagesQueryHandler : IRequestHandler<GetMessagesQuery, Result<PagedResult<MessageDto>>>
    {
        private readonly AppDbContext _context;

        public GetMessagesQueryHandler(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Result<PagedResult<MessageDto>>> Handle(GetMessagesQuery request, CancellationToken cancellationToken)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == request.UserId, cancellationToken);
            if (user == null || user.IsDeleted)
            {
                return new Result<PagedResult<MessageDto>>
                {
                    Succeeded = false,
                    ErrorCode = ErrorCodes.AccountDeleted,
                    Message = "Account not found or is deleted."
                };
            }

            // Verify conversation and participation
            var chat = await _context.Chats
                .FirstOrDefaultAsync(c => c.Id == request.ChatId && (c.ClientId == request.UserId || c.FreelancerId == request.UserId), cancellationToken);

            if (chat == null)
            {
                return new Result<PagedResult<MessageDto>>
                {
                    Succeeded = false,
                    ErrorCode = "CONVERSATION_NOT_FOUND",
                    Message = $"Conversation with ID {request.ChatId} not found or you are not a participant."
                };
            }

            using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);

            try
            {
                // Mark unread messages from other users as read
                var unreadMessages = await _context.Messages
                    .Where(m => m.ChatId == request.ChatId && 
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
                    .Where(m => m.ChatId == request.ChatId)
                    .OrderByDescending(m => m.SentAt);

                var totalCount = await query.CountAsync(cancellationToken);
                var items = await query
                    .Skip((request.PageNumber - 1) * request.PageSize)
                    .Take(request.PageSize)
                    .Select(m => new MessageDto
                    {
                        Id = m.Id,
                        ChatId = m.ChatId,
                        SenderId = m.SenderId,
                        Body = m.Body,
                        Status = m.Status,
                        SentAt = m.SentAt,
                        Type = m.Type,
                        TextContent = m.TextContent,
                        FileUrl = m.FileUrl,
                        FileName = m.FileName,
                        FileSizeBytes = m.FileSizeBytes
                    })
                    .ToListAsync(cancellationToken);

                await transaction.CommitAsync(cancellationToken);

                var response = new PagedResult<MessageDto>
                {
                    Items = items,
                    TotalCount = totalCount,
                    Page = request.PageNumber,
                    PageSize = request.PageSize
                };

                return new Result<PagedResult<MessageDto>> { Succeeded = true, Data = response };
            }
            catch (Exception)
            {
                await transaction.RollbackAsync(cancellationToken);
                throw;
            }
        }
    }
}

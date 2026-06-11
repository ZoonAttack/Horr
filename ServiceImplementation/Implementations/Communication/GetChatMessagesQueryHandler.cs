using Entities;
using Entities.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ServiceContracts.DTOs.Chat;
using ServiceContracts.DTOs.Responses;
using ServiceImplementation.Helpers;
using Services;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ServiceImplementation.Mappings.Communication;

namespace ServiceImplementation.Implementations.Communication
{
    public class GetChatMessagesQueryHandler : IRequestHandler<GetChatMessagesQuery, Result<PagedResult<MessageDto>>>
    {
        private readonly AppDbContext _context;

        public GetChatMessagesQueryHandler(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Result<PagedResult<MessageDto>>> Handle(GetChatMessagesQuery request, CancellationToken cancellationToken)
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
                .FirstOrDefaultAsync(c => c.Id == request.ChatId && 
                                          (c.ClientId == request.UserId || c.FreelancerId == request.UserId), 
                                     cancellationToken);

            if (chat == null)
            {
                return new Result<PagedResult<MessageDto>>
                {
                    Succeeded = false,
                    ErrorCode = ErrorCodes.Unauthorized,
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
                    .Include(m => m.Sender)
                    .Where(m => m.ChatId == request.ChatId)
                    .OrderByDescending(m => m.SentAt)
                    .ThenByDescending(m => m.Id);

                var totalCount = await query.CountAsync(cancellationToken);
                
                var rawItems = await query
                    .Skip((request.Page - 1) * request.PageSize)
                    .Take(request.PageSize)
                    .ToListAsync(cancellationToken);

                var items = rawItems.Select(m => m.ToDto()).ToList();

                await transaction.CommitAsync(cancellationToken);

                var response = new PagedResult<MessageDto>
                {
                    Items = items,
                    TotalCount = totalCount,
                    Page = request.Page,
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

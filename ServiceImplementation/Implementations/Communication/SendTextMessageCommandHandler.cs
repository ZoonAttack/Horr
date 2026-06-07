using Entities;
using Entities.Communication;
using Entities.Enums;
using MediatR;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using ServiceContracts.DTOs.Chat;
using ServiceContracts.DTOs.Responses;
using ServiceImplementation.Helpers;
using ServiceImplementation.Hubs;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace ServiceImplementation.Implementations.Communication
{
    public class SendTextMessageCommandHandler : IRequestHandler<SendTextMessageCommand, Result<MessageDto>>
    {
        private readonly AppDbContext _context;
        private readonly IHubContext<ChatHub> _hubContext;

        public SendTextMessageCommandHandler(AppDbContext context, IHubContext<ChatHub> hubContext)
        {
            _context = context;
            _hubContext = hubContext;
        }

        public async Task<Result<MessageDto>> Handle(SendTextMessageCommand request, CancellationToken cancellationToken)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == request.SenderId, cancellationToken);
            if (user == null || user.IsDeleted)
            {
                return new Result<MessageDto>
                {
                    Succeeded = false,
                    ErrorCode = ErrorCodes.AccountDeleted,
                    Message = "Sender account not found or is deleted."
                };
            }

            var chat = await _context.Chats.FirstOrDefaultAsync(c => c.Id == request.ChatId, cancellationToken);
            if (chat == null)
            {
                return new Result<MessageDto>
                {
                    Succeeded = false,
                    ErrorCode = "CONVERSATION_NOT_FOUND",
                    Message = $"Chat with ID {request.ChatId} not found."
                };
            }

            if (chat.ClientId != request.SenderId && chat.FreelancerId != request.SenderId)
            {
                return new Result<MessageDto>
                {
                    Succeeded = false,
                    ErrorCode = ErrorCodes.Unauthorized,
                    Message = "You are not a participant in this chat."
                };
            }

            var message = new Message
            {
                Id = Guid.NewGuid().ToString(),
                ChatId = request.ChatId,
                SenderId = request.SenderId,
                Body = request.Text ?? string.Empty,
                Status = MessageStatus.Unread,
                SentAt = DateTime.UtcNow,
                Type = MessageType.Text,
                TextContent = request.Text
            };

            _context.Messages.Add(message);
            await _context.SaveChangesAsync(cancellationToken);

            // Set the sender navigation property from the loaded user to map it correctly
            message.Sender = user;
            var messageDto = message.ToMessageDto();

            // Broadcast to group
            await _hubContext.Clients.Group(request.ChatId)
                .SendAsync("ReceiveMessage", messageDto, cancellationToken);

            return new Result<MessageDto> { Succeeded = true, Data = messageDto };
        }
    }
}

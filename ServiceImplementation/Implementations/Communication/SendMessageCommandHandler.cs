using Entities;
using Entities.Communication;
using Entities.Enums;
using ServiceImplementation.Hubs;
using MediatR;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using ServiceContracts.DTOs.Chat;
using ServiceImplementation.Exceptions;
using ServiceContracts.DTOs.Responses;
using ServiceImplementation.Helpers;
using System.IO;

namespace ServiceImplementation.Implementations.Communication
{
    public class SendMessageCommandHandler : IRequestHandler<SendMessageCommand, Result<MessageDto>>
    {
        private readonly AppDbContext _context;
        private readonly IHubContext<ChatHub> _hubContext;

        public SendMessageCommandHandler(AppDbContext context, IHubContext<ChatHub> hubContext)
        {
            _context = context;
            _hubContext = hubContext;
        }

        public async Task<Result<MessageDto>> Handle(SendMessageCommand request, CancellationToken cancellationToken)
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

            using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);

            try
            {
                var conversation = await _context.Conversations
                    .FirstOrDefaultAsync(c => c.Id == request.ConversationId, cancellationToken);

                if (conversation == null)
                {
                    // If the conversation doesn't exist, we must have JobPostId and ReceiverId to initiate it
                    if (string.IsNullOrEmpty(request.JobPostId) || string.IsNullOrEmpty(request.ReceiverId))
                    {
                        return new Result<MessageDto>
                        {
                            Succeeded = false,
                            ErrorCode = "CONVERSATION_NOT_FOUND",
                            Message = $"Conversation with ID {request.ConversationId} not found, and no JobPostId or ReceiverId was provided to initiate a new one."
                        };
                    }

                    // Verify that the JobPost exists
                    var jobExists = await _context.JobPosts.AnyAsync(j => j.Id == request.JobPostId && !j.IsDeleted, cancellationToken);
                    if (!jobExists)
                    {
                        return new Result<MessageDto>
                        {
                            Succeeded = false,
                            ErrorCode = ErrorCodes.JobNotFound,
                            Message = "Job post not found or is deleted."
                        };
                    }

                    // Verify that the Receiver exists
                    var receiverExists = await _context.Users.AnyAsync(u => u.Id == request.ReceiverId && !u.IsDeleted, cancellationToken);
                    if (!receiverExists)
                    {
                        return new Result<MessageDto>
                        {
                            Succeeded = false,
                            ErrorCode = ErrorCodes.UserNotFound,
                            Message = "Recipient account not found or is deleted."
                        };
                    }

                    // Create new conversation linked to the job post
                    conversation = new Conversation
                    {
                        Id = request.ConversationId,
                        JobPostId = request.JobPostId,
                        CreatedAt = DateTime.UtcNow
                    };
                    _context.Conversations.Add(conversation);

                    // Add participants
                    var participant1 = new ConversationParticipant
                    {
                        ConversationId = request.ConversationId,
                        UserId = request.SenderId,
                        JoinedAt = DateTime.UtcNow
                    };
                    var participant2 = new ConversationParticipant
                    {
                        ConversationId = request.ConversationId,
                        UserId = request.ReceiverId,
                        JoinedAt = DateTime.UtcNow
                    };
                    _context.ConversationParticipants.AddRange(participant1, participant2);

                    // Save the conversation details so we can add the message in the transaction
                    await _context.SaveChangesAsync(cancellationToken);
                }

                // Create Message
                var message = new Message
                {
                    Id = Guid.NewGuid().ToString(),
                    ConversationId = request.ConversationId,
                    SenderId = request.SenderId,
                    Body = request.Body,
                    Status = MessageStatus.Unread,
                    SentAt = DateTime.UtcNow
                };

                _context.Messages.Add(message);

                // Handle file uploads
                if (request.Files != null && request.Files.Count > 0)
                {
                    var uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "chat");
                    if (!Directory.Exists(uploadPath))
                    {
                        Directory.CreateDirectory(uploadPath);
                    }

                    foreach (var file in request.Files)
                    {
                        if (file.Length > 0)
                        {
                            var fileName = $"{Guid.NewGuid()}_{file.FileName}";
                            var filePath = Path.Combine(uploadPath, fileName);

                            using (var stream = new FileStream(filePath, FileMode.Create))
                            {
                                await file.CopyToAsync(stream, cancellationToken);
                            }

                            var attachment = new Attachment
                            {
                                Id = Guid.NewGuid().ToString(),
                                Message = message,
                                FileUrl = $"/uploads/chat/{fileName}",
                                FileType = Path.GetExtension(file.FileName),
                                UploadedAt = DateTime.UtcNow
                            };
                            _context.Attachments.Add(attachment);
                        }
                    }
                }

                await _context.SaveChangesAsync(cancellationToken);

                var messageDto = new MessageDto
                {
                    Id = message.Id,
                    ConversationId = message.ConversationId,
                    SenderId = message.SenderId,
                    Body = message.Body,
                    Status = message.Status,
                    SentAt = message.SentAt
                };

                // Broadcast
                await _hubContext.Clients.Group(request.ConversationId)
                    .SendAsync("ReceiveMessage", messageDto, cancellationToken);

                await transaction.CommitAsync(cancellationToken);

                return new Result<MessageDto> { Succeeded = true, Data = messageDto };
            }
            catch (Exception)
            {
                await transaction.RollbackAsync(cancellationToken);
                throw;
            }
        }
    }
}

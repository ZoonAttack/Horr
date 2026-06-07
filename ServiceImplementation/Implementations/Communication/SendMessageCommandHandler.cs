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
using Microsoft.AspNetCore.Hosting;

namespace ServiceImplementation.Implementations.Communication
{
    public class SendMessageCommandHandler : IRequestHandler<SendMessageCommand, Result<MessageDto>>
    {
        private readonly AppDbContext _context;
        private readonly IHubContext<ChatHub> _hubContext;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public SendMessageCommandHandler(AppDbContext context, IHubContext<ChatHub> hubContext, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _hubContext = hubContext;
            _webHostEnvironment = webHostEnvironment;
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
                var chat = await _context.Chats
                    .FirstOrDefaultAsync(c => c.Id == request.ChatId, cancellationToken);

                if (chat == null)
                {
                    // If the chat doesn't exist, we must have ContractId and ReceiverId to initiate it
                    if (request.ContractId == null || string.IsNullOrEmpty(request.ReceiverId))
                    {
                        return new Result<MessageDto>
                        {
                            Succeeded = false,
                            ErrorCode = "CONVERSATION_NOT_FOUND",
                            Message = $"Chat with ID {request.ChatId} not found, and no ContractId or ReceiverId was provided to initiate a new one."
                        };
                    }

                    // Verify that the Contract exists
                    var contract = await _context.Contracts.FirstOrDefaultAsync(c => c.Id == request.ContractId, cancellationToken);
                    if (contract == null)
                    {
                        return new Result<MessageDto>
                        {
                            Succeeded = false,
                            ErrorCode = ErrorCodes.ContractNotFound,
                            Message = "Contract not found or is deleted."
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

                    // Create new chat linked to the contract
                    chat = new Chat
                    {
                        Id = request.ChatId,
                        ContractId = contract.Id,
                        ClientId = contract.ClientId,
                        FreelancerId = contract.FreelancerId,
                        CreatedAt = DateTime.UtcNow,
                        IsDeleted = false
                    };
                    _context.Chats.Add(chat);

                    // Save the chat details so we can add the message in the transaction
                    await _context.SaveChangesAsync(cancellationToken);
                }

                // Determine MessageType and primary file info
                MessageType msgType = MessageType.Text;
                string? textContent = request.Body;

                if (request.Files != null && request.Files.Count > 0)
                {
                    var firstFile = request.Files[0];
                    var ext = Path.GetExtension(firstFile.FileName).ToLower();
                    if (ext == ".png" || ext == ".jpg" || ext == ".jpeg" || ext == ".gif" || ext == ".webp")
                    {
                        msgType = MessageType.Image;
                    }
                    else if (ext == ".mp4" || ext == ".mov" || ext == ".avi" || ext == ".webm" || ext == ".mkv")
                    {
                        msgType = MessageType.Video;
                    }
                    else if (ext == ".pdf")
                    {
                        msgType = MessageType.Pdf;
                    }
                }

                // Create Message
                var message = new Message
                {
                    Id = Guid.NewGuid().ToString(),
                    ChatId = request.ChatId,
                    SenderId = request.SenderId,
                    Body = request.Body ?? string.Empty,
                    Status = MessageStatus.Unread,
                    SentAt = DateTime.UtcNow,
                    Type = msgType,
                    TextContent = textContent
                };

                _context.Messages.Add(message);

                // Handle file uploads saving
                if (request.Files != null && request.Files.Count > 0)
                {
                    var webRootPath = _webHostEnvironment.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
                    var uploadPath = Path.Combine(webRootPath, "uploads", "chat", request.ChatId);
                    
                    if (!Directory.Exists(uploadPath))
                    {
                        Directory.CreateDirectory(uploadPath);
                    }

                    for (int i = 0; i < request.Files.Count; i++)
                    {
                        var file = request.Files[i];
                        if (file.Length > 0)
                        {
                            var extension = Path.GetExtension(file.FileName);
                            var uniqueFileName = $"{Guid.NewGuid()}{extension}";
                            var filePath = Path.Combine(uploadPath, uniqueFileName);

                            using (var stream = new FileStream(filePath, FileMode.Create))
                            {
                                await file.CopyToAsync(stream, cancellationToken);
                            }

                            var fileUrlPath = $"/uploads/chat/{request.ChatId}/{uniqueFileName}";

                            if (i == 0)
                            {
                                message.FileUrl = fileUrlPath;
                                message.FileName = file.FileName;
                                message.FileSizeBytes = file.Length;
                            }

                            var attachment = new Attachment
                            {
                                Id = Guid.NewGuid().ToString(),
                                Message = message,
                                FileUrl = fileUrlPath,
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
                    ChatId = message.ChatId,
                    SenderId = message.SenderId,
                    Body = message.Body,
                    Status = message.Status,
                    SentAt = message.SentAt,
                    Type = message.Type,
                    TextContent = message.TextContent,
                    FileUrl = message.FileUrl,
                    FileName = message.FileName,
                    FileSizeBytes = message.FileSizeBytes
                };

                // Broadcast
                await _hubContext.Clients.Group(request.ChatId)
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

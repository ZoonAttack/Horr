using Entities;
using Entities.Communication;
using Entities.Enums;
using MediatR;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using ServiceContracts.DTOs.Chat;
using ServiceContracts.DTOs.Responses;
using ServiceImplementation.Helpers;
using ServiceImplementation.Hubs;
using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ServiceImplementation.Mappings.Communication;

namespace ServiceImplementation.Implementations.Communication
{
    public class UploadChatFileCommandHandler : IRequestHandler<UploadChatFileCommand, Result<MessageDto>>
    {
        private readonly AppDbContext _context;
        private readonly IHubContext<ChatHub> _hubContext;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public UploadChatFileCommandHandler(AppDbContext context, IHubContext<ChatHub> hubContext, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _hubContext = hubContext;
            _webHostEnvironment = webHostEnvironment;
        }

        public async Task<Result<MessageDto>> Handle(UploadChatFileCommand request, CancellationToken cancellationToken)
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
                    ErrorCode = ErrorCodes.ChatNotFound,
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

            var file = request.File;
            if (file == null || file.Length == 0)
            {
                return new Result<MessageDto>
                {
                    Succeeded = false,
                    ErrorCode = ErrorCodes.InvalidFile,
                    Message = "No file uploaded or file is empty."
                };
            }

            // Extension validation
            var ext = Path.GetExtension(file.FileName).ToLower();
            MessageType msgType;
            long limit = 0;

            if (ext == ".png" || ext == ".jpg" || ext == ".jpeg" || ext == ".gif" || ext == ".webp")
            {
                msgType = MessageType.Image;
                limit = 10 * 1024 * 1024; // 10MB
            }
            else if (ext == ".mp4" || ext == ".mov" || ext == ".avi" || ext == ".webm")
            {
                msgType = MessageType.Video;
                limit = 150 * 1024 * 1024; // 150MB
            }
            else if (ext == ".pdf")
            {
                msgType = MessageType.Pdf;
                limit = 20 * 1024 * 1024; // 20MB
            }
            else
            {
                return new Result<MessageDto>
                {
                    Succeeded = false,
                    ErrorCode = ErrorCodes.InvalidFileType,
                    Message = $"File extension {ext} is not allowed."
                };
            }

            if (file.Length > limit)
            {
                return new Result<MessageDto>
                {
                    Succeeded = false,
                    ErrorCode = ErrorCodes.FileTooLarge,
                    Message = $"File exceeds the allowed limit of {limit / (1024 * 1024)}MB."
                };
            }

            using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);

            try
            {
                var webRootPath = _webHostEnvironment.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
                var uploadPath = Path.Combine(webRootPath, "uploads", "chat", request.ChatId);

                if (!Directory.Exists(uploadPath))
                {
                    Directory.CreateDirectory(uploadPath);
                }

                var uniqueFileName = $"{Guid.NewGuid()}{ext}";
                var filePath = Path.Combine(uploadPath, uniqueFileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream, cancellationToken);
                }

                var fileUrlPath = $"/uploads/chat/{request.ChatId}/{uniqueFileName}";

                var message = new Message
                {
                    Id = Guid.NewGuid().ToString(),
                    ChatId = request.ChatId,
                    SenderId = request.SenderId,
                    Body = file.FileName,
                    Status = MessageStatus.Unread,
                    SentAt = DateTime.UtcNow,
                    Type = msgType,
                    FileUrl = fileUrlPath,
                    FileName = file.FileName,
                    FileSizeBytes = file.Length
                };

                _context.Messages.Add(message);

                var attachment = new Attachment
                {
                    Id = Guid.NewGuid().ToString(),
                    Message = message,
                    FileUrl = fileUrlPath,
                    FileType = ext,
                    UploadedAt = DateTime.UtcNow
                };
                _context.Attachments.Add(attachment);

                await _context.SaveChangesAsync(cancellationToken);

                message.Sender = user;
                var messageDto = message.ToDto();

                // Broadcast
                await _hubContext.Clients.Group($"chat-{request.ChatId}")
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

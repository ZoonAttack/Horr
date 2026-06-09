using Entities.Communication;
using ServiceContracts.DTOs.Chat;

namespace ServiceImplementation.Mappings.Communication
{
    public static class MessageMappingExtensions
    {
        public static MessageDto ToDto(this Message message)
        {
            if (message == null) return null!;

            return new MessageDto
            {
                Id = message.Id.ToString(),
                MessageId = message.Id.ToString(),
                ChatId = message.ChatId.ToString(),
                SenderId = message.SenderId,
                SenderName = message.Sender?.FullName ?? string.Empty,
                SenderAvatarUrl = message.Sender?.ProfilePicturePath,
                Body = message.Body,
                Status = message.Status,
                SentAt = message.SentAt,
                Type = message.Type,
                TextContent = message.TextContent,
                FileUrl = message.FileUrl,
                FileName = message.FileName,
                FileSizeBytes = message.FileSizeBytes
            };
        }
    }
}

using Entities.Communication;
using Entities.Enums;
using ServiceContracts.DTOs.Chat;

namespace ServiceImplementation.Mappings.Communication
{
    /// <summary>
    /// Static extension methods for mapping Conversation, Message, and Attachment
    /// entities to their corresponding DTOs.
    /// No AutoMapper — all mappings are manual.
    /// </summary>
    public static class ConversationMappingExtensions
    {
        /// <summary>
        /// Maps a <see cref="Chat"/> to a <see cref="ConversationDto"/>.
        /// Requires the <c>Messages</c> navigation property to be loaded.
        /// </summary>
        public static ConversationDto ToDto(this Chat chat)
        {
            if (chat == null) return null!;

            var lastMessage = chat.Messages
                .Where(m => !m.IsDeleted)
                .OrderByDescending(m => m.SentAt)
                .FirstOrDefault();

            var unreadCount = chat.Messages
                .Count(m => !m.IsDeleted && m.Status == MessageStatus.Unread);

            return new ConversationDto
            {
                Id = chat.Id,
                CreatedAt = chat.CreatedAt,
                LastMessagePreview = MessagePreviewHelper.GetPreview(lastMessage?.Body),
                UnreadCount = unreadCount
            };
        }

        /// <summary>
        /// Maps a <see cref="Message"/> to a <see cref="MessageDto"/>.
        /// </summary>
        public static MessageDto ToDto(this Message message)
        {
            if (message == null) return null!;

            return new MessageDto
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
        }

        /// <summary>
        /// Maps an <see cref="Attachment"/> to an <see cref="AttachmentDto"/>.
        /// </summary>
        public static AttachmentDto ToDto(this Attachment attachment)
        {
            if (attachment == null) return null!;

            return new AttachmentDto
            {
                Id = attachment.Id,
                MessageId = attachment.MessageId,
                FileUrl = attachment.FileUrl,
                FileType = attachment.FileType,
                UploadedAt = attachment.UploadedAt
            };
        }
    }
}

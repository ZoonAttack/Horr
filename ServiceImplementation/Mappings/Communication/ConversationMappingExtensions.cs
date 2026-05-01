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
        /// Maps a <see cref="Conversation"/> to a <see cref="ConversationDto"/>.
        /// Requires the <c>Messages</c> navigation property to be loaded.
        /// </summary>
        public static ConversationDto ToDto(this Conversation conversation)
        {
            if (conversation == null) return null!;

            var lastMessage = conversation.Messages
                .Where(m => !m.IsDeleted)
                .OrderByDescending(m => m.SentAt)
                .FirstOrDefault();

            var unreadCount = conversation.Messages
                .Count(m => !m.IsDeleted && m.Status == MessageStatus.Unread);

            return new ConversationDto
            {
                Id = conversation.Id,
                CreatedAt = conversation.CreatedAt,
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
                ConversationId = message.ConversationId,
                SenderId = message.SenderId,
                Body = message.Body,
                Status = message.Status,
                SentAt = message.SentAt
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

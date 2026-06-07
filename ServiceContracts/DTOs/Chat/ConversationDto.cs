using Entities.Enums;

namespace ServiceContracts.DTOs.Chat
{
    /// <summary>
    /// Read DTO for a Conversation, including a preview of the last message
    /// and the count of unread messages for the requesting user.
    /// </summary>
    public class ConversationDto
    {
        public string Id { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; }

        /// <summary>Truncated preview of the last message body (max 53 chars).</summary>
        public string LastMessagePreview { get; set; } = string.Empty;

        /// <summary>Number of unread messages in this conversation.</summary>
        public int UnreadCount { get; set; }
    }

    /// <summary>
    /// DTO for a Chat Summary in lists.
    /// </summary>
    public class ChatSummaryDto
    {
        public string Id { get; set; } = string.Empty;
        public string ChatId { get; set; } = string.Empty;
        public int ContractId { get; set; }
        public string OtherPartyName { get; set; } = string.Empty;
        public string? OtherPartyAvatarUrl { get; set; }
        public string LastMessagePreview { get; set; } = string.Empty;
        public DateTime? LastMessageAt { get; set; }
        public int UnreadCount { get; set; }
    }

    /// <summary>
    /// Read DTO for a Message.
    /// </summary>
    public class MessageDto
    {
        public string Id { get; set; } = string.Empty;

        public string MessageId { get; set; } = string.Empty;

        public string ChatId { get; set; } = string.Empty;

        public string SenderId { get; set; } = string.Empty;

        public string SenderName { get; set; } = string.Empty;

        public string? SenderAvatarUrl { get; set; }

        public string Body { get; set; } = string.Empty;

        public MessageStatus Status { get; set; }

        public DateTime SentAt { get; set; }

        public MessageType Type { get; set; }

        public string? TextContent { get; set; }

        public string? FileUrl { get; set; }

        public string? FileName { get; set; }

        public long? FileSizeBytes { get; set; }
    }

    /// <summary>
    /// Read DTO for an Attachment.
    /// </summary>
    public class AttachmentDto
    {
        public string Id { get; set; } = string.Empty;

        public string MessageId { get; set; } = string.Empty;

        public string FileUrl { get; set; } = string.Empty;

        public string FileType { get; set; } = string.Empty;

        public DateTime UploadedAt { get; set; }
    }
}

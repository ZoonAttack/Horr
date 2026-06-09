using Entities.Enums;
using System;

namespace ServiceContracts.DTOs.Chat
{
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
        public MessageType Type { get; set; }
        public string? TextContent { get; set; }
        public string? FileUrl { get; set; }
        public string? FileName { get; set; }
        public long? FileSizeBytes { get; set; }
        public DateTime SentAt { get; set; }
    }
}

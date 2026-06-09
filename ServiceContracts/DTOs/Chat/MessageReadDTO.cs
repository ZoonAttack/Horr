using Entities.Enums;

namespace ServiceContracts.DTOs.Chat
{
    /// <summary>
    /// DTO for reading or displaying message information.
    /// </summary>
    public class MessageReadDTO
    {
        public string Id { get; set; }

        public string ChatId { get; set; }

        public string SenderId { get; set; }

        public string Body { get; set; }

        public MessageStatus Status { get; set; }

        public DateTime SentAt { get; set; }

        public MessageType Type { get; set; }

        public string? TextContent { get; set; }

        public string? FileUrl { get; set; }

        public string? FileName { get; set; }

        public long? FileSizeBytes { get; set; }
    }
}

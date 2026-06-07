using System.ComponentModel.DataAnnotations;
using Entities.Enums;

namespace ServiceContracts.DTOs.Chat
{
    /// <summary>
    /// DTO for creating a new Message.
    /// </summary>
    public class MessageCreateDTO
    {
        [Required]
        public string ChatId { get; set; }

        [Required]
        public string SenderId { get; set; }

        [Required]
        public string Body { get; set; }

        public MessageType Type { get; set; } = MessageType.Text;

        public string? TextContent { get; set; }

        public string? FileUrl { get; set; }

        public string? FileName { get; set; }

        public long? FileSizeBytes { get; set; }
    }
}

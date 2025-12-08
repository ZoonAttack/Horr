using System.ComponentModel.DataAnnotations;

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

        public string Content { get; set; }

        [MaxLength(255)]
        public string AttachmentUrl { get; set; }

        [MaxLength(50)]
        public string AttachmentType { get; set; }
    }
}

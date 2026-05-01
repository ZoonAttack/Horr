using System.ComponentModel.DataAnnotations;

namespace ServiceContracts.DTOs.Chat
{
    /// <summary>
    /// DTO for creating a new Message.
    /// </summary>
    public class MessageCreateDTO
    {
        [Required]
        public string ConversationId { get; set; }

        [Required]
        public string SenderId { get; set; }

        [Required]
        public string Body { get; set; }
    }
}

using Entities.Enums;

namespace ServiceContracts.DTOs.Chat
{
    /// <summary>
    /// DTO for updating message information (primarily read status).
    /// </summary>
    public class MessageUpdateDTO
    {
        public MessageStatus Status { get; set; }
    }
}

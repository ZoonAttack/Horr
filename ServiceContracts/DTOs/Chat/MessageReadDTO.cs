namespace ServiceContracts.DTOs.Chat
{
    /// <summary>
    /// DTO for reading or displaying message information.
    /// </summary>
    public class MessageReadDTO
    {
        public long Id { get; set; }

        public long ChatId { get; set; }

        public long SenderId { get; set; }

        public string Content { get; set; }

        public string AttachmentUrl { get; set; }

        public string AttachmentType { get; set; }

        public bool IsRead { get; set; }

        public DateTime SentAt { get; set; }
    }
}

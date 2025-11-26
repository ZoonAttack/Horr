namespace ServiceContracts.DTOs.Chat
{
    /// <summary>
    /// DTO for reading or displaying chat information.
    /// </summary>
    public class ChatReadDTO
    {
        public long Id { get; set; }

        public long ProjectId { get; set; }

        public long ClientId { get; set; }

        public long FreelancerId { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}

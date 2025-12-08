namespace ServiceContracts.DTOs.Chat
{
    /// <summary>
    /// DTO for reading or displaying chat information.
    /// </summary>
    public class ChatReadDTO
    {
        public string Id { get; set; }

        public string ProjectId { get; set; }

        public string ClientId { get; set; }

        public string FreelancerId { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}

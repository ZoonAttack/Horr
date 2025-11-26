using System.ComponentModel.DataAnnotations;

namespace ServiceContracts.DTOs.Chat
{
    /// <summary>
    /// DTO for creating a new Chat room.
    /// </summary>
    public class ChatCreateDTO
    {
        [Required]
        public long ProjectId { get; set; }

        [Required]
        public long ClientId { get; set; }

        [Required]
        public long FreelancerId { get; set; }
    }
}

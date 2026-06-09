using System.ComponentModel.DataAnnotations;

namespace ServiceContracts.DTOs.Chat
{
    /// <summary>
    /// DTO for creating a new Chat room.
    /// </summary>
    public class ChatCreateDTO
    {
        [Required]
        public int ContractId { get; set; }

        [Required]
        public string ClientId { get; set; }

        [Required]
        public string FreelancerId { get; set; }
    }
}

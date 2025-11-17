namespace ServiceContracts.DTOs.User.Client
{
    /// <summary>
    /// DTO for reading/displaying Client profile information,
    /// </summary>
    public class ClientReadDTO
    {
        public long UserId { get; set; }

        public string FullName { get; set; }

        public string Email { get; set; }

        public string Phone { get; set; }

        public string Bio { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime UpdatedAt { get; set; }
    }
}

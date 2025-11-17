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

        /// <summary>
        /// Converts from Client entity and linked User entity to DTO
        /// </summary>
        /// <param name="client"></param>
        /// <param name="user"></param>
        /// <returns></returns>
        public static ClientReadDTO Client_To_ClientRead(Entities.User.Client client, Entities.User.User user)
        {
            return new ClientReadDTO
            {
                UserId = client.UserId,
                FullName = user.FullName,
                Email = user.Email,
                Phone = user.Phone,
                Bio = client.Bio,
                CreatedAt = client.CreatedAt,
                UpdatedAt = client.UpdatedAt
            };
        }
    }
}

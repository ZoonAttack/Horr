using Entities;
using ServiceContracts.DTOs.User.Client;

namespace ServiceContracts.DTOs.User.Client
{
    public static class ClientExtensions
    {
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

        public static void ApplyToClient(this ClientUpdateDTO dto, Entities.User.Client client)
        {
            if (dto.Bio != null)
                client.Bio = dto.Bio;
        }
    }
}

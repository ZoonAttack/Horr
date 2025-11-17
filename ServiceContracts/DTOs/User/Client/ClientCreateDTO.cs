using System.ComponentModel.DataAnnotations;

namespace ServiceContracts.DTOs.User.Client
{
    /// <summary>
    /// DTO for creating a new Client profile linked to a User.
    /// </summary>
    public class ClientCreateDTO
    {
        [MaxLength(1000)]
        public string Bio { get; set; }

        /// <summary>
        /// Convert to Client entity
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public Entities.User.Client ClientCreate_To_Client(long userId)
        {
            return new Entities.User.Client
            {
                UserId = userId,
                Bio = this.Bio
            };
        }
    }
}

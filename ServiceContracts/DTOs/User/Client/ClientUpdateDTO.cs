using System.ComponentModel.DataAnnotations;

namespace ServiceContracts.DTOs.User.Client
{
    /// <summary>
    /// DTO for updating an existing Client profile.
    /// </summary>
    public class ClientUpdateDTO
    {
        [MaxLength(1000)]
        public string Bio { get; set; }

        /// <summary>
        /// Applies updates to the Client entity
        /// </summary>
        /// <param name="client"></param>
        public void ClinetUpdate_To_Client(Entities.User.Client client)
        {
            if (this.Bio != null)
                client.Bio = this.Bio;
        }
    }
}

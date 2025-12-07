using System.ComponentModel.DataAnnotations;
using Entities.User;

namespace ServiceContracts.DTOs.User.User
{
    /// <summary>
    /// DTO for updating existing User account details.
    /// Limited to modifiable fields like name and phone.
    /// Does not allow role or password change here.
    /// </summary>
    public class UserUpdateDTO
    {
        [MaxLength(150)]
        public string FullName { get; set; }

        [Phone]
        [MaxLength(20)]
        public string Phone { get; set; }

        /// <summary>
        /// Applies update DTO properties to an existing User entity
        /// </summary>
        /// <param name="user"></param>
        public void UserUpdate_To_User(Entities.User.User user)
        {
            if (!string.IsNullOrEmpty(this.FullName))
                user.FullName = this.FullName;

            if (!string.IsNullOrEmpty(this.Phone))
                user.Phone = this.Phone;
        }
    }
}

using System;
using System.ComponentModel.DataAnnotations;
using Entities.Enums;
using Entities.User;

namespace ServiceContracts.DTOs.User.User
{
    /// <summary>
    /// DTO for creating a new User account.
    /// </summary>
    public class UserCreateDTO
    {
        [Required]
        [MaxLength(150)]
        public string FullName { get; set; }

        [Required]
        [EmailAddress]
        [MaxLength(150)]
        public string Email { get; set; }

        [Required]
        [Phone]
        [MaxLength(20)]
        public string Phone { get; set; }

        [Required]
        [MaxLength(255)]
        public string Password { get; set; }

        [Required]
        [MaxLength(20)]
        public UserRole Role { get; set; }  // Example values: "client", "freelancer", "specialist"

        /// <summary>
        /// Converts UserCreateDTO to User entity (for creation)
        /// </summary>
        /// <returns></returns>
        public Entities.User.User UserCreate_To_User()
        {
            return new Entities.User.User
            {
                FullName = this.FullName,
                Email = this.Email,
                Phone = this.Phone,
                // Password should be hashed later in the service layer
                PasswordHash = this.Password,
                Role = this.Role,
                IsVerified = false,
                TrustScore = 0,
            };
        }

    }
}

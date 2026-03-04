using Entities.Enums;
using Entities.Users;

namespace Services.DTOs.UserDTOs.User
{
    /// <summary>
    /// DTO for reading or displaying user information.
    /// Omits sensitive data like password.
    /// Includes system-generated fields and status flags.
    /// </summary>
    public class UserReadDTO
    {
        public string Id { get; set; }

        public string FullName { get; set; }

        public string Email { get; set; }

        public string Phone { get; set; }

        public UserRole Role { get; set; }

        public bool IsVerified { get; set; }

        public decimal TrustScore { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime UpdatedAt { get; set; }
    }

    public static class UserExtensions
    {
        /// <summary>
        /// Converts User entity to UserReadDTO
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        public static UserReadDTO User_To_UserRead(Entities.Users.User user)
        {
            return new UserReadDTO
            {
                Id = user.Id,
                FullName = user.FullName,
                Email = user.Email,
                Phone = user.PhoneNumber,
                Role = user.Role,
                IsVerified = user.IsVerified,
                CreatedAt = user.CreatedAt,
                UpdatedAt = user.UpdatedAt
            };
        }
    }
}

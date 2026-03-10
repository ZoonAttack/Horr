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

        public string? Address { get; set; }
        public string? City { get; set; }
        public string? StateProvince { get; set; }
        public string? ZipCode { get; set; }
        public string? Country { get; set; }
        public string? TimeZone { get; set; }
        public string? Bio { get; set; }

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
                TrustScore = user.TrustScore,
                Address = user.Address,
                City = user.City,
                StateProvince = user.StateProvince,
                ZipCode = user.ZipCode,
                Country = user.Country,
                TimeZone = user.TimeZone,
                Bio = user.Bio,
                CreatedAt = user.CreatedAt,
                UpdatedAt = user.UpdatedAt
            };
        }
    }
}

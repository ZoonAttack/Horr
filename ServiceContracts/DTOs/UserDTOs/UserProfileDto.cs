using Entities.Enums;

namespace ServiceContracts.DTOs.UserDTOs
{
    public class UserProfileDto
    {
        public string Id { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string? Email { get; set; }
        public string? PhoneNumber { get; set; }
        
        // Location
        public string? Address { get; set; }
        public string? City { get; set; }
        public string? StateProvince { get; set; }
        public string? ZipCode { get; set; }
        public string? Country { get; set; }
        public string? TimeZone { get; set; }
        
        // Profile Info
        public string? Title { get; set; }
        public string? Bio { get; set; }
        public decimal TrustScore { get; set; }
        public bool IsVerified { get; set; }
        
        // Pending Email (used when email update is requested but not confirmed)
        public string? PendingEmail { get; set; }
        
        // Privacy settings (derived from Freelancer, if applicable)
        public Visibility? Visibility { get; set; }
        public ExperienceLevel? ExperienceLevel { get; set; }
        public string? UserIdHash { get; set; }
    }

    public static class UserProfileDtoExtensions
    {
        public static UserProfileDto ToUserProfileDto(this Entities.Users.User user, string? pendingEmail = null, Entities.Users.Freelancer? freelancer = null)
        {
            if (user == null) return null!;
            return new UserProfileDto
            {
                Id = user.Id,
                FullName = user.FullName,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                Address = user.Address,
                City = user.City,
                StateProvince = user.StateProvince,
                ZipCode = user.ZipCode,
                Country = user.Country,
                TimeZone = user.TimeZone,
                Title = freelancer?.Title,
                Bio = freelancer?.Bio,
                TrustScore = user.TrustScore,
                IsVerified = user.IsVerified,
                PendingEmail = pendingEmail,
                Visibility = freelancer?.VisibilityPreference,
                ExperienceLevel = freelancer?.ExperienceLevel,
                UserIdHash = user.Id?.Length >= 8 ? user.Id.Substring(0, 8) : user.Id
            };
        }
    }
}

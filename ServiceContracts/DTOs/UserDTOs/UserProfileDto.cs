using Entities.Enums;
using ServiceContracts.DTOs.UserDTOs.FreelancerManagement;

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
        public ExperienceLevel? ExperienceLevel { get; set; }
        public int? YearsOfExperience { get; set; }
        public string? UserIdHash { get; set; }

        // Professional Details
        public string? Availability { get; set; }
        public decimal? HourlyRate { get; set; }
        public string? PortfolioUrl { get; set; }
        public List<LanguageReadDto> Languages { get; set; } = new List<LanguageReadDto>();
        public List<EducationReadDto> Education { get; set; } = new List<EducationReadDto>();
        public List<ExperienceDetailReadDto> ExperienceDetails { get; set; } = new List<ExperienceDetailReadDto>();
        public List<EmploymentReadDto> EmploymentHistory { get; set; } = new List<EmploymentReadDto>();
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
                ExperienceLevel = freelancer?.ExperienceLevel,
                YearsOfExperience = freelancer?.YearsOfExperience,
                UserIdHash = user.Id?.Length >= 8 ? user.Id.Substring(0, 8) : user.Id,
                Availability = freelancer?.Availability,
                HourlyRate = freelancer?.HourlyRate,
                PortfolioUrl = freelancer?.PortfolioUrl,
                Languages = freelancer?.Languages?.Select(l => new LanguageReadDto { Id = l.Id, Name = l.Name, Level = l.Level }).ToList() ?? new List<LanguageReadDto>(),
                Education = freelancer?.Education?.Select(e => new EducationReadDto { Id = e.Id, School = e.School, Degree = e.Degree, FieldOfStudy = e.FieldOfStudy, DateStart = e.DateStart, DateEnd = e.DateEnd }).ToList() ?? new List<EducationReadDto>(),
                ExperienceDetails = freelancer?.ExperienceDetails?.Select(e => new ExperienceDetailReadDto { Id = e.Id, Subject = e.Subject, Description = e.Description }).ToList() ?? new List<ExperienceDetailReadDto>(),
                EmploymentHistory = freelancer?.EmploymentHistory?.Select(e => new EmploymentReadDto { Id = e.Id, Company = e.Company, Title = e.Title, City = e.City, Country = e.Country, FromDate = e.FromDate, ToDate = e.ToDate, CurrentlyWorkThere = e.CurrentlyWorkThere }).ToList() ?? new List<EmploymentReadDto>()
            };
        }
    }
}

using ServiceContracts.DTOs.FreelancerProfile;
using System.Collections.Generic;

namespace ServiceContracts.DTOs.UserDTOs
{
    public class PublicProfileDto
    {
        public string FullName { get; set; } = string.Empty;
        public string? Title { get; set; }
        public string? Bio { get; set; }
        public string? City { get; set; }
        public string? Country { get; set; }
        public string? ProfilePicturePath { get; set; }
        public decimal TrustScore { get; set; }
        public bool IsVerified { get; set; }
        public int ExperienceLevel { get; set; }
        public int? YearsOfExperience { get; set; }
        
        // Stats
        public string TotalEarnings { get; set; } = "$0";
        public int TotalJobs { get; set; } = 0;
        public int TotalHours { get; set; } = 0;

        public List<string> Skills { get; set; } = new List<string>();          // skill names only
        public List<PortfolioItemDto> Portfolio { get; set; } = new List<PortfolioItemDto>();
        public List<LanguageDto> Languages { get; set; } = new List<LanguageDto>();
        public List<EducationDto> Education { get; set; } = new List<EducationDto>();
        public List<EmploymentDto> EmploymentHistory { get; set; } = new List<EmploymentDto>();
    }

    public class LanguageDto
    {
        public string Name { get; set; } = string.Empty;
        public string Level { get; set; } = string.Empty;
    }

    public class EducationDto
    {
        public string School { get; set; } = string.Empty;
        public string Degree { get; set; } = string.Empty;
        public string FieldOfStudy { get; set; } = string.Empty;
        public int Year { get; set; }
    }

    public class EmploymentDto
    {
        public string Company { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string DateRange { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
    }
}

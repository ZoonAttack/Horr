using System.Collections.Generic;
using ServiceContracts.DTOs.Skill.FreelancerSkill;

namespace ServiceContracts.DTOs.UserDTOs.FreelancerManagement
{
    public class FreelancerPublicReadDTO
    {
        // --- Core User Properties (Public Read) ---
        public string Id { get; set; }
        public string FullName { get; set; }
        public string? ProfilePicturePath { get; set; }
        public bool IsVerified { get; set; }
        public decimal TrustScore { get; set; }
        public double AverageRating { get; set; }
        public int TotalReviews { get; set; }
        public int JobSuccessPercentage { get; set; }

        // --- Freelancer Profile Properties ---
        public string? Title { get; set; }
        public string? Bio { get; set; }
        public decimal? HourlyRate { get; set; }
        public string Availability { get; set; }
        public int? YearsOfExperience { get; set; }
        public string PortfolioUrl { get; set; }

        public string? City { get; set; }
        public string? Country { get; set; }

        // --- Profile Collections (Read DTOs) ---
        public ICollection<LanguageReadDto> Languages { get; set; } = new List<LanguageReadDto>();
        public ICollection<EducationReadDto> Education { get; set; } = new List<EducationReadDto>();
        public ICollection<ExperienceDetailReadDto> ExperienceDetails { get; set; } = new List<ExperienceDetailReadDto>();
        public ICollection<EmploymentReadDto> EmploymentHistory { get; set; } = new List<EmploymentReadDto>();
        public ICollection<FreelancerSkillReadDTO> Skills { get; set; } = new List<FreelancerSkillReadDTO>();
    }
}

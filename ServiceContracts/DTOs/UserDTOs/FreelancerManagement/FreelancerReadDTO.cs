using System;
using System.Collections.Generic;
using Entities.Enums;
using ServiceContracts.DTOs.Skill.FreelancerSkill;
// Assuming your helper DTOs (LanguageReadDto, etc.) are in the same namespace or referenced.

namespace ServiceContracts.DTOs.UserDTOs.FreelancerManagement
{
    public class FreelancerReadDTO
    {
        // --- Core User Properties (Read) ---
        public string Id { get; set; }
        public UserRole Role { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public string? ProfilePicturePath { get; set; }
        public bool IsVerified { get; set; }
        public decimal TrustScore { get; set; }
        public double AverageRating { get; set; }
        public int TotalReviews { get; set; }
        public int JobSuccessPercentage { get; set; }
        public bool IsSaved { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        // --- Freelancer Profile Properties ---
        public string? Title { get; set; }
        public string? Bio { get; set; }
        public decimal? HourlyRate { get; set; }
        public string Availability { get; set; }
        public int? YearsOfExperience { get; set; }
        public string PortfolioUrl { get; set; }

        public string? Address { get; set; }
        public string? City { get; set; }
        public string? StateProvince { get; set; }
        public string? ZipCode { get; set; }
        public string? Country { get; set; }
        public string? TimeZone { get; set; }

        // --- NEW PROFILE COLLECTIONS for Read ---

        // 1. Languages [name and level for each one]
        public ICollection<LanguageReadDto> Languages { get; set; } = new List<LanguageReadDto>();

        // 2. Education details [school, date of start and end, degree, field of study]
        public ICollection<EducationReadDto> Education { get; set; } = new List<EducationReadDto>();

        // 3. Experience details [subject & description]
        public ICollection<ExperienceDetailReadDto> ExperienceDetails { get; set; } = new List<ExperienceDetailReadDto>();

        // 4. Employment [company, city, country, title, currently work there or not, from date, to date]
        public ICollection<EmploymentReadDto> EmploymentHistory { get; set; } = new List<EmploymentReadDto>();

        // 5. Skills tags
        public ICollection<FreelancerSkillReadDTO> Skills { get; set; } = new List<FreelancerSkillReadDTO>();
    }
}

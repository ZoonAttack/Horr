using System;
using System.Collections.Generic;
// Assuming your helper DTOs (LanguageCreateDto, etc.) are in the same namespace or referenced.

namespace ServiceContracts.DTOs.User.Freelancer
{
    public class FreelancerCreateDTO
    {
        // --- Core User Properties (Write) ---
        public string FullName { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string Password { get; set; }

        // --- Freelancer Profile Properties ---
        public string Bio { get; set; }
        public decimal? HourlyRate { get; set; }
        public string Availability { get; set; }
        public int? YearsOfExperience { get; set; }
        public string PortfolioUrl { get; set; }

        // --- Profile Collections (Updated to use *CreateDto) ---

        // 1. Languages
        public ICollection<LanguageCreateDto> Languages { get; set; } = new List<LanguageCreateDto>();

        // 2. Education Details
        public ICollection<EducationCreateDto> Education { get; set; } = new List<EducationCreateDto>();

        // 3. Experience Details
        public ICollection<ExperienceDetailCreateDto> ExperienceDetails { get; set; } = new List<ExperienceDetailCreateDto>();

        // 4. Employment History
        public ICollection<EmploymentCreateDto> EmploymentHistory { get; set; } = new List<EmploymentCreateDto>();
    }
}
using System;
using System.Collections.Generic;

namespace ServiceContracts.DTOs.UserDTOs.FreelancerManagement
{
    public class FreelancerUpdateDTO
    {
        // --- Core User Properties (Write) ---
        public string? FullName { get; set; }
        public string? Email { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Address { get; set; }
        public string? City { get; set; }
        public string? StateProvince { get; set; }
        public string? ZipCode { get; set; }
        public string? Country { get; set; }
        public string? TimeZone { get; set; }

        // --- Freelancer Profile Properties ---
        public string? Title { get; set; }
        public string? Bio { get; set; }
        public decimal? HourlyRate { get; set; }
        public string? Availability { get; set; }
        public int? YearsOfExperience { get; set; }
        public string? PortfolioUrl { get; set; }
        public int? ExperienceLevel { get; set; }

        // --- NEW PROFILE COLLECTIONS for Update ---

        // 1. Languages [name and level for each one]
        public ICollection<LanguageUpdateDto>? Languages { get; set; }

        // 2. Education details [school, date of start and end, degree, field of study]
        public ICollection<EducationUpdateDto>? Education { get; set; }

        // 3. Experience details [subject & description]
        public ICollection<ExperienceDetailUpdateDto>? ExperienceDetails { get; set; }

        // 4. Employment [company, city, country, title, currently work there or not, from date, to date]
        public ICollection<EmploymentUpdateDto>? EmploymentHistory { get; set; }
    }
}

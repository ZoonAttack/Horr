using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;
// Assuming your helper DTOs (LanguageCreateDto, etc.) are properly defined

namespace ServiceContracts.DTOs.User.Freelancer
{
    public class FreelancerCreateDTO
    {
        // --- Core User Properties (Inferred from best practices) ---

        [Required(ErrorMessage = "Full Name is required.")]
        [StringLength(100, ErrorMessage = "Full Name cannot exceed 100 characters.")]
        public string FullName { get; set; }

        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Invalid Email format.")]
        public string Email { get; set; }

        [Phone(ErrorMessage = "Invalid Phone number format.")]
        // MaxLength is often needed here too, e.g., [StringLength(20)]
        public string Phone { get; set; }

        [Required(ErrorMessage = "Password is required.")]
        // Enforce basic security requirements (e.g., minimum length)
        [MinLength(8, ErrorMessage = "Password must be at least 8 characters.")]
        public string Password { get; set; }

        // --- Freelancer Profile Properties (Mirrors Freelancer Entity) ---

        // Note: [Column(TypeName = "text")] for Bio allows large text, so no StringLength is strictly needed.
        public string Bio { get; set; }

        // Matches decimal(10,2) format, must be positive, and allows null (?)
        [Range(0.01, 99999999.99, ErrorMessage = "Hourly Rate must be between 0.01 and 99,999,999.99.")]
        public decimal? HourlyRate { get; set; }

        [StringLength(50, ErrorMessage = "Availability cannot exceed 50 characters.")]
        public string Availability { get; set; }

        // Allows null (?), and validation ensures it's a positive number
        [Range(0, 100, ErrorMessage = "Years of Experience must be between 0 and 100.")]
        public int? YearsOfExperience { get; set; }

        // Mirrors [MaxLength(255)] and [Url] attributes
        [StringLength(255, ErrorMessage = "Portfolio URL cannot exceed 255 characters.")]
        [Url(ErrorMessage = "Invalid URL format.")]
        public string PortfolioUrl { get; set; }

        // --- Profile Collections ---

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
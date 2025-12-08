using Entities.Enums;

namespace ServiceContracts.DTOs.User.Freelancer
{
    public class FreelancerReadDTO
    {
        // --- Core User Properties (Read) ---
        public string Id { get; set; }
        public UserRole Role { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public bool IsVerified { get; set; }
        public decimal TrustScore { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        // --- Freelancer Profile Properties ---
        public string Bio { get; set; }
        public decimal? HourlyRate { get; set; }
        public string Availability { get; set; }
        public int? YearsOfExperience { get; set; }
        public string PortfolioUrl { get; set; }
    }
}
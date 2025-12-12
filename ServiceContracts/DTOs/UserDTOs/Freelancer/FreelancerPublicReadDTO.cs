namespace ServiceContracts.DTOs.User.Freelancer
{
    public class FreelancerPublicReadDTO
    {
        // --- Core User Properties (Public Read) ---
        public string Id { get; set; }
        public string FullName { get; set; }

        // --- Freelancer Profile Properties ---
        public string Bio { get; set; }
        public decimal? HourlyRate { get; set; }
        public string Availability { get; set; }
        public int? YearsOfExperience { get; set; }
        public string PortfolioUrl { get; set; }
        public bool IsVerified { get; set; }
        public decimal TrustScore { get; set; }

        // --- Profile Collections (Read DTOs) ---
        public ICollection<LanguageReadDto> Languages { get; set; } = new List<LanguageReadDto>();
        public ICollection<EducationReadDto> Education { get; set; } = new List<EducationReadDto>();
        public ICollection<ExperienceDetailReadDto> ExperienceDetails { get; set; } = new List<ExperienceDetailReadDto>();
        public ICollection<EmploymentReadDto> EmploymentHistory { get; set; } = new List<EmploymentReadDto>();
    }
}